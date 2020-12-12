using DatabaseEngine.Commands;
using DatabaseEngine.LogicalPlan;
using DatabaseEngine.Operations;
using DatabaseEngine.PhysicalPlan;
using DatabaseEngine.Relations;
using DatabaseEngine.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DatabaseEngine
{
    public class Program
    {
        public static string StorageFilePath = $"{Directory.GetCurrentDirectory()}\\data.storage";
        public static RelationManager RelationManager { get; set; }
        public static StatisticsManager StatisticsManager { get; set; }
        public static MemoryManager MemoryManager { get; set; }
        public static bool Debug = true;
        public static LogicalQueryPlan LogicalQueryPlan { get; set; }
        public static PhysicalQueryPlan PhysicalQueryPlan { get; set; }
        public static string UserQuery { get; set; }

        unsafe static void Main(string[] args)
        {
            File.Delete(StorageFilePath);
            StorageFile storageFile = new StorageFile(StorageFilePath);

            MemoryManager = new MemoryManager(storageFile);

            RelationManager = new RelationManager(MemoryManager);
            StatisticsManager = new StatisticsManager(RelationManager);

            RelationManager.Initialize();
            CreateProductsTableIfNotExists(RelationManager);
            CreateProducersTableIfNotExists(RelationManager);

            StatisticsManager.CalculateStatistics();
            StatisticsManager.PrintStatistics();

            //string query = "SELECT products.BuildYear, * FROM products JOIN producers on products.producer = producers.name WHERE producers.Name = \"AMD\" ";
            //string query = "SELECT TOP 1000 * FROM products WHERE products.Producer IN (SELECT Name FROM producers WHERE Id = 2)";
            //string query = "SELECT * FROM products JOIN producers on products.producer = producers.name WHERE (producers.name = \"AMD\" && products.BuildYear = 2010)";
            UserQuery = "SELECT * FROM products WHERE Id = 1";
            //string query = "SELECT products.BuildYear, * FROM products JOIN producers on products.producer = producers.name WHERE producers.Name = \"AMD\"";
            while (!string.IsNullOrEmpty(UserQuery))
            {
                Console.WriteLine("Executing query...");

                List<CustomTuple> result = ExecuteQuery(UserQuery, true);

                if (result.Count > 0)
                {
                    List<string> columnNames = new List<string>();
                    foreach (CustomObject entry in result[0].Entries)
                    {
                        columnNames.Add(entry.AttributeDefinition.Name.ToString());
                    }
                    Console.WriteLine("[" + string.Join("|", columnNames) + "]");
                }

                foreach (CustomTuple tuple in result)
                {
                    List<string> s = new List<string>();

                    foreach (CustomObject entry in tuple.Entries)
                    {
                        s.Add(entry.Value.ToString());
                    }

                    Console.WriteLine(string.Join("|", s));
                }

                Console.WriteLine("Enter new query:");
                UserQuery = Console.ReadLine();
            }
        }


        public static List<CustomTuple> ExecuteQuery(string query, bool debug = false)
        {
            if (LogicalQueryPlan == null)
            {
                LogicalQueryPlan = new LogicalQueryPlan(RelationManager);
            }
            if (PhysicalQueryPlan == null)
            {
                PhysicalQueryPlan = new PhysicalQueryPlan(RelationManager, StatisticsManager);
            }

            if (Program.Debug)
            {
                Console.WriteLine("[DEBUG]: Query: " + query);
            }
            LogicalElement logicalTree = LogicalQueryPlan.GetTreeForQuery(query);

            List<CustomTuple> results = new List<CustomTuple>();

            if (logicalTree == null)
            {
                return results;
            }

            List<LogicalElement> postorderLogicalTree = AppendPostOrder(new List<LogicalElement>(), logicalTree);

            QueryPlanNode root = new QueryPlanNode(null, null);

            List<QueryPlanNode> lastOptions = new List<QueryPlanNode> { root };

            for (int i = 0; i < postorderLogicalTree.Count; i++)
            {
                LogicalElement element = postorderLogicalTree[i];

                Dictionary<QueryPlanNode, int> options = GetOptions(element);
                ConnectEndNodesToNodes(lastOptions, options.Select(x => x.Key));

                lastOptions = options.Select(x => x.Key).ToList();
            }

            List<QueryPlanNode> leastCostPath = GetLeastCostPath(root);

            PhysicalOperation physicalOperation = PhysicalQueryPlan.CreateFromPath(logicalTree, leastCostPath);


            if (query == UserQuery)
            {
                ;
            }

            if (debug)
            {
                //string s = element.ToDot();
                //;
            }

            physicalOperation.Prepare();


            CustomTuple result;
            do
            {
                result = physicalOperation.GetNext();

                if (result != null)
                {
                    results.Add(result);
                }
            }
            while (result != null);

            //QueryPlan plan = new QueryPlan(command);

            //int reads = MemoryBuffer.Reads;
            //int writes = MemoryManager.Writes;
            //List<CustomTuple> result = plan.Execute();
            //if (Program.Debug)
            //{
            //    Console.WriteLine("[DEBUG]: Reads for last query: " + (MemoryBuffer.Reads - reads) + " (total: " + reads + "), writes: " + (MemoryManager.Writes - writes) + " (total: " + writes + ")");
            //}

            return results;
        }

        private static void ConnectEndNodesToNodes(IEnumerable<QueryPlanNode> nodes, IEnumerable<QueryPlanNode> endNodes)
        {
            foreach (QueryPlanNode node in nodes)
            {
                ConnectEndNodesToNodes(node, endNodes);
            }
        }

        private static void ConnectEndNodesToNodes(QueryPlanNode node, IEnumerable<QueryPlanNode> endNodes)
        {
            if (node.Edges.Count == 0)
            {
                foreach(QueryPlanNode endNode in endNodes)
                {
                    node.Edges.Add(new QueryPlanNodeEdge
                    {
                        From = node,
                        To = endNode,
                        Cost = 0
                    });
                }
            }
            else
            {

                foreach (QueryPlanNode node1 in node.Edges.Select(x => x.To))
                {
                    ConnectEndNodesToNodes(node1, endNodes);
                }
            }
        }

        private static Dictionary<QueryPlanNode, int> GetOptions(LogicalElement element)
        {
            Dictionary<QueryPlanNode, int> options = new Dictionary<QueryPlanNode, int>();

            if (element is ProjectionElement p)
            {
                PhysicalOperation proj = new ProjectionOperation(element, null, p.Columns.Select(x => x.AttributeDefinition).ToList());
                options.Add(new QueryPlanNode(element, proj), proj.GetCost());
            }
            else if (element is SelectionElement selectionElement)
            {
                if (selectionElement.LeftChild is RelationElement relationElement)
                {
                    Table table = Program.RelationManager.GetTable(relationElement.Relation.Id);

                    PhysicalOperation tableScan = new TableScanOperation(element, table);
                    if (selectionElement.Condition != null)
                    {
                        PhysicalOperation f = new FilterOperation(element, tableScan, selectionElement.Condition);
                        options.Add(new QueryPlanNode(element, f), f.GetCost());
                    }
                    else
                    {
                        options.Add(new QueryPlanNode(element, tableScan), tableScan.GetCost());
                    }

                    Condition clonedCondition = selectionElement.Condition?.Clone();

                    if (TryExtractConstantConditionWithIndex(relationElement.Relation, clonedCondition, out LeafCondition constantCondition))
                    {
                        selectionElement.Condition = selectionElement.Condition.Simplify();

                        PhysicalOperation indexSeek = new IndexSeekOperation(element, table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column), constantCondition);

                        if (selectionElement.Condition != null)
                        {
                            PhysicalOperation f = new FilterOperation(element, indexSeek, clonedCondition);
                            options.Add(new QueryPlanNode(element, f), f.GetCost());
                        }
                        else
                        {
                            options.Add(new QueryPlanNode(element, indexSeek), indexSeek.GetCost());
                        }
                    }
                    else
                    {
                        if (table.TableDefinition.HasClusteredIndex())
                        {
                            PhysicalOperation indexSeek = new IndexSeekOperation(element, table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column), constantCondition);

                            if (selectionElement.Condition != null)
                            {
                                PhysicalOperation f = new FilterOperation(element, indexSeek, selectionElement.Condition);
                                options.Add(new QueryPlanNode(element, f), f.GetCost());
                            }
                            else
                            {
                                options.Add(new QueryPlanNode(element, indexSeek), indexSeek.GetCost());
                            }
                        }
                    }
                }
            }
            else if (element is RelationElement relElement)
            {
                Table table = Program.RelationManager.GetTable(relElement.Relation.Id);

                if (table.TableDefinition.HasClusteredIndex())
                {
                    PhysicalOperation indexSeek = new IndexScanOperation(element, table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column));
                    options.Add(new QueryPlanNode(element, indexSeek), indexSeek.GetCost());
                }
                else
                {
                    PhysicalOperation tableScan = new TableScanOperation(element, table);
                    options.Add(new QueryPlanNode(element, tableScan), tableScan.GetCost());
                }
            }

            return options;
        }

        private static List<LogicalElement> AppendPostOrder(List<LogicalElement> result, LogicalElement center)
        {
            if (center.LeftChild != null)
            {
                AppendPostOrder(result, center.LeftChild);
            }
            if (center.RightChild != null)
            {
                AppendPostOrder(result, center.RightChild);
            }
            result.Add(center);

            return result;
        }


        private static List<QueryPlanNode> GetLeastCostPath(QueryPlanNode root)
        {
            QueryPlanNode cur = root;
            List<QueryPlanNode> result = new List<QueryPlanNode>();
            result.Add(cur);

            while(cur != null)
            {
                QueryPlanNodeEdge cheapestEdge = cur.Edges.OrderBy(x => x.Cost).FirstOrDefault();
                if (cheapestEdge != null)
                {
                    cur = cheapestEdge.To;

                    if (cur != null)
                    {
                        result.Add(cur);
                    }
                }
                else
                {
                    cur = null;
                }
            }

            return result;
        }

        private static bool TryExtractConstantConditionWithIndex(Relation relation, Condition condition, out LeafCondition result)
        {
            result = null;

            if (condition is AndCondition andCondition)
            {
                if (TryExtractConstantConditionWithIndex(relation, andCondition.Left, out result))
                {
                    return true;
                }
                else if (TryExtractConstantConditionWithIndex(relation, andCondition.Right, out result))
                {
                    return true;
                }

                return false;
            }
            else if (condition is LeafCondition leaf
                && leaf.Operation == Compiler.Common.RelOp.Equals)
            {
                foreach (Index index in (relation as TableDefinition).Indexes)
                {
                    if (leaf.Column == (relation as TableDefinition).GetAttributeByName(index.Column))
                    {
                        result = new LeafCondition()
                        {
                            Column = leaf.Column,
                            Operation = leaf.Operation,
                            Value = leaf.Value
                        };

                        leaf.AlwaysTrue = true;

                        return true;
                    }
                }
            }

            return false;
        }

        private static void CreateProducersTableIfNotExists(RelationManager relationManager)
        {
            if (!relationManager.TableExists("Producers"))
            {
                TableDefinition table = new TableDefinition()
                {
                    Name = "Producers",
                    Id = 2
                };

                table.Add(new AttributeDefinition() { Name = "Id", Type = ValueType.Integer });
                table.Add(new AttributeDefinition() { Name = "Name", Type = ValueType.String });

                relationManager.CreateTable(table);
                WriteProducers();
            }
        }

        private static void CreateProductsTableIfNotExists(RelationManager relationManager)
        {
            if (!relationManager.TableExists("Products"))
            {
                TableDefinition table = new TableDefinition()
                {
                    Name = "Products",
                    Id = 1
                };

                table.Add(new AttributeDefinition() { Name = "Id", Type = ValueType.Integer });
                table.Add(new AttributeDefinition() { Name = "BuildYear", Type = ValueType.Integer });
                table.Add(new AttributeDefinition() { Name = "Producer", Type = ValueType.String });
                table.AddIndex(new Index { IsClustered = true, Column = "Id" });
                //table.AddIndex(new Index { IsClustered = false, Column = "Producer" });

                relationManager.CreateTable(table);
                WriteProducts();
            }
        }

        private static void WriteProducers()
        {
            //ExecuteQuery("INSERT INTO producers VALUES (1, \"Intel\")");
            //ExecuteQuery("INSERT INTO producers VALUES (2, \"AMD\")");

            Table table = RelationManager.GetTable("producers");
            table.Insert(new CustomTuple(table.TableDefinition).AddValueFor<int>("Id", 1).AddValueFor("Name", "Intel"));
            table.Insert(new CustomTuple(table.TableDefinition).AddValueFor<int>("Id", 2).AddValueFor("Name", "AMD"));
        }

        private static void WriteProducts()
        {
            //ExecuteQuery("INSERT INTO products VALUES (1, 1994, \"Intel\")");
            //ExecuteQuery("INSERT INTO products VALUES (2, 2010, \"AMD\")");
            //ExecuteQuery("INSERT INTO products VALUES (4, 2020, \"AMD\")");
            //ExecuteQuery("INSERT INTO products VALUES (3, 2015, \"Intel\")");

            Table table = RelationManager.GetTable("products");
            //table.Insert(new CustomTuple(table.TableDefinition).AddValueFor<int>("Id", 1).AddValueFor("BuildYear", 1994).AddValueFor("Producer", "Intel"));
            //table.Insert(new CustomTuple(table.TableDefinition).AddValueFor<int>("Id", 2).AddValueFor("BuildYear", 2010).AddValueFor("Producer", "AMD"));
            //table.Insert(new CustomTuple(table.TableDefinition).AddValueFor<int>("Id", 3).AddValueFor("BuildYear", 2020).AddValueFor("Producer", "AMD"));
            //table.Insert(new CustomTuple(table.TableDefinition).AddValueFor<int>("Id", 4).AddValueFor("BuildYear", 2015).AddValueFor("Producer", "Intel"));

            table.StartBulkMode();
            for(int i = 0; i < 10; i++)
            {
                table.Insert(new CustomTuple(table.TableDefinition).AddValueFor<int>("Id", i).AddValueFor("BuildYear", 1900 + i/5).AddValueFor("Producer", i % 2 == 0 ? "Intel" : "AMD"));
            }
            table.EndBulkMode();

            //RelationManager.GetTable("products").StartBulkMode();
            //for(int i = 0; i < 1000; i++)
            //{
            //    ExecuteQuery("INSERT INTO products VALUES (" + i + ", " + i + ", \"Intel\")");
            //}
            //RelationManager.GetTable("products").EndBulkMode();
        }
    }
}

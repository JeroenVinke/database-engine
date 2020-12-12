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

            List<LogicalElement> postorderLogicalTree = AppendPostOrder(new List<LogicalElement>(), logicalTree);


            //List<BBNode> queue = new List<BBNode>();
            //QueryPlanBBGraph graph = new QueryPlanBBGraph
            //{
            //    Root = new QueryPlanBBNode(logicalTree, null)
            //};

            //graph.Expand(graph.Root);

            //List<BBNode> leastCostPath = GetLeastCostPath(root);


            return new List<CustomTuple>();
            List<CustomTuple> results = new List<CustomTuple>();

            if (logicalTree == null)
            {
                return results;
            }

            //if (logicalTree is ReadLogicalElement readLogicalElement)
            //{
            //    int t = readLogicalElement.T();
            //}
            if (query == UserQuery)
            {
                ;
            }
            PhysicalOperation physicalTree = PhysicalQueryPlan.GetFromLogicalTree(logicalTree);

            if (debug)
            {
                //string s = element.ToDot();
                //;
            }

            physicalTree.Prepare();


            CustomTuple result;
            do
            {
                result = physicalTree.GetNext();

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

        //private class QueryPlanBBGraph : BBGraph
        //{
        //    public override void Expand(BBNode node)
        //    {
        //        QueryPlanBBNode bbNode = (QueryPlanBBNode)node;

        //        if (bbNode.LogicalElement.LeftChild != null)
        //        {
        //            Dictionary<QueryPlanBBNode, int> leftOptions = GetOptions(bbNode.LogicalElement.LeftChild);

        //            foreach (KeyValuePair<QueryPlanBBNode, int> leftOption in leftOptions)
        //            {
        //                bbNode.Edges.Add(new BBEdge
        //                {
        //                    From = bbNode,
        //                    To = leftOption.Key,
        //                    Cost = leftOption.Value
        //                });

        //                if (bbNode.LogicalElement.LeftChild != null)
        //                {
        //                    Dictionary<QueryPlanBBNode, int> rightOptions = GetOptions(bbNode.LogicalElement.RightChild);

        //                    bbNode.Edges.Add(new BBEdge
        //                    {
        //                        From = left,
        //                        To = right,
        //                        Cost = right.Edges.Min(x => x.Cost)
        //                    });
        //                }
        //            }

        //        }
        //    }

        //    public QueryPlanBBNode Expand(LogicalElement element)
        //    {
        //        if (element == null)
        //        {
        //            return null;
        //        }


                


        //        ConnectEndNodesToNodes(o1.Select(x => x.Key), endNode);

        //        return startNode;
        //    }

        //    public QueryPlanBBNode ExpandChildren(QueryPlanBBNode bbNode)
        //    {
        //        QueryPlanBBNode left = Expand(bbNode.LogicalElement.LeftChild);
        //        QueryPlanBBNode right = Expand(bbNode.LogicalElement.RightChild);

        //        bbNode.Edges.Add(new BBEdge
        //        {
        //            From = bbNode,
        //            To = left,
        //            Cost = left.Edges.Min(x => x.Cost)
        //        });

        //        return bbNode;
        //    }

        //    private void ConnectEndNodesToNode(IEnumerable<QueryPlanBBNode> nodes, EndBBNode endNode)
        //    {
        //        foreach(QueryPlanBBNode node in nodes)
        //        {
        //            ConnectEndNodesToNode(node, endNode);
        //        }
        //    }

        //    private void ConnectEndNodesToNode(QueryPlanBBNode node, EndBBNode endNode)
        //    {
        //        if (node.Edges.Count == 0) 
        //        {
        //            node.Edges.Add(new BBEdge
        //            {
        //                From = node,
        //                To = endNode,
        //                Cost = 0
        //            });
        //        }
        //    }

        //    private Dictionary<QueryPlanBBNode, int> GetOptions(LogicalElement element)
        //    {
        //        Dictionary<QueryPlanBBNode, int> options = new Dictionary<QueryPlanBBNode, int>();

        //        if (element is ProjectionElement p)
        //        {
        //            PhysicalOperation proj = new ProjectionOperation(null, p.Columns.Select(x => x.AttributeDefinition).ToList());
        //            options.Add(new QueryPlanBBNode(element, proj), proj.GetCost());
        //        }
        //        else if (element is SelectionElement selectionElement)
        //        {
        //            if (selectionElement.LeftChild is RelationElement relationElement)
        //            {
        //                Table table = Program.RelationManager.GetTable(relationElement.Relation.Id);

        //                if (TryExtractConstantConditionWithIndex(relationElement.Relation, selectionElement.Condition, out LeafCondition constantCondition))
        //                {
        //                    selectionElement.Condition = selectionElement.Condition.Simplify();

        //                    PhysicalOperation indexSeek = new IndexSeekOperation(table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column), constantCondition);

        //                    if (selectionElement.Condition != null)
        //                    {
        //                        PhysicalOperation f = new FilterOperation(indexSeek, selectionElement.Condition);
        //                        options.Add(new QueryPlanBBNode(element, f), f.GetCost());
        //                    }
        //                    else
        //                    {
        //                        options.Add(new QueryPlanBBNode(element, indexSeek), indexSeek.GetCost());
        //                    }
        //                }
        //                else
        //                {
        //                    if (table.TableDefinition.HasClusteredIndex())
        //                    {
        //                        PhysicalOperation indexSeek = new IndexSeekOperation(table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column), constantCondition);

        //                        if (selectionElement.Condition != null)
        //                        {
        //                            PhysicalOperation f = new FilterOperation(indexSeek, selectionElement.Condition);
        //                            options.Add(new QueryPlanBBNode(element, f), f.GetCost());
        //                        }
        //                        else
        //                        {
        //                            options.Add(new QueryPlanBBNode(element, indexSeek), indexSeek.GetCost());
        //                        }
        //                    }
        //                    else
        //                    {
        //                        PhysicalOperation tableScan = new TableScanOperation(table);
        //                        if (selectionElement.Condition != null)
        //                        {
        //                            PhysicalOperation f = new FilterOperation(tableScan, selectionElement.Condition);
        //                            options.Add(new QueryPlanBBNode(element, f), f.GetCost());
        //                        }
        //                        else
        //                        {
        //                            options.Add(new QueryPlanBBNode(element, tableScan), tableScan.GetCost());
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        return options;
        //    }
        //}

        private static List<BBNode> GetLeastCostPath(BBNode root)
        {
            BBNode cur = root;
            List<BBNode> result = new List<BBNode>();
            result.Add(cur);

            while(cur != null)
            {
                BBEdge cheapestEdge = cur.Edges.OrderBy(x => x.Cost).FirstOrDefault();
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

        

        //private static void ExpandChildren(LogicalElement cur, BBNode curBBNode)
        //{
        //    List<BBNode> leftOptions = GetPhysicalOptions(cur.LeftChild);
        //    foreach (BBNode option in leftOptions)
        //    {
        //        curBBNode.Edges.Add(new BBEdge
        //        {
        //            From = curBBNode,
        //            To = option
        //        });
        //    }

        //    List<BBNode> rightOptions = GetPhysicalOptions(cur.RightChild);
        //    foreach (BBNode option2 in rightOptions)
        //    {
        //        foreach (BBNode option1 in leftOptions)
        //        {
        //            curBBNode.Edges.Add(new BBEdge
        //            {
        //                From = option1,
        //                To = option2
        //            });
        //        }
        //    }
        //}


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

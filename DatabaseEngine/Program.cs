using Compiler.LexicalAnalyer;
using Compiler.Parser;
using Compiler.Parser.SyntaxTreeNodes;
using DatabaseEngine.Commands;
using DatabaseEngine.Operations;
using DatabaseEngine.Relations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DatabaseEngine
{
    public class Program
    {
        public static string StorageFilePath = $"{Directory.GetCurrentDirectory()}\\data.storage";

        unsafe static void Main(string[] args)
        {
            File.Delete(StorageFilePath);
            StorageFile storageFile = new StorageFile(StorageFilePath);

            RelationManager relationManager = new RelationManager(storageFile);
            CreateProductsTableIfNotExists(relationManager);
            CreateMakersTableIfNotExists(relationManager);
            
            WriteProducers(relationManager.GetTable("Producers"));
            ReadProducers(relationManager.GetTable("Producers"));

            WriteProducts(relationManager.GetTable("Products"));
            ReadProducts(relationManager.GetTable("Products"));


            string query = "select * from producers join products on producers.name == products.producer where Name == \"AMD\" ";
            while (!string.IsNullOrEmpty(query))
            {
                Console.WriteLine("Query: " + query);

                Command command = ParseCommand(relationManager, query);

                QueryPlan plan = new QueryPlan(command);

                List<CustomTuple> result = plan.Execute();

                foreach(CustomTuple tuple in result)
                {
                    List<string> s = new List<string>();

                    foreach(CustomObject entry in tuple.Entries)
                    {
                        s.Add(entry.Value.ToString());
                    }

                    Console.WriteLine(string.Join("|", s));
                }

                Console.WriteLine("Enter new query....");
                query = Console.ReadLine();
            }

            
        }

        public static Command ParseCommand(RelationManager relationManager, string query)
        {
            LexicalAnalyzer analyzer = new LexicalAnalyzer(LexicalLanguage.GetLanguage(), query);
            BottomUpParser parser = new BottomUpParser(analyzer);

            parser.Parse();
            parser.OutputDebugFiles();

            SyntaxTreeNode command = parser.TopLevelAST;

            if (command is SelectASTNode selectCommandAST)
            {
                Table table = relationManager.GetTable(selectCommandAST.From.Identifier.Identifier);

                //Set result = IndexSearch(table, null);

                SelectCommand selectCommand = new SelectCommand
                {
                    Table = table,
                    Condition = BooleanExpressionToCondition(table.TableDefinition, selectCommandAST.Condition),
                    Join = JoinNodeToJoin(relationManager, table, selectCommandAST.Join)
                };

                return selectCommand;
            }

            return null;
        }

        private static Join JoinNodeToJoin(RelationManager relationManager, Table leftTable, JoinASTNode join)
        {
            Table rightTable = relationManager.GetTable(join.TargetTable.Identifier);

            return new Join
            {
                LeftTable = leftTable,
                RightTable = rightTable,
                LeftColumn = leftTable.TableDefinition.GetAttributeByName(join.LeftColumn.Identifier.Split(".")[1]),
                RightColumn = rightTable.TableDefinition.GetAttributeByName(join.RightColumn.Identifier.Split(".")[1])
            };
        }

        private static void CreateMakersTableIfNotExists(RelationManager relationManager)
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
                table.AddClusteredIndex(new List<AttributeDefinition>
                {
                    table.First(x => x.Name == "Id" )
                }, 0);
                table.AddNonClusteredIndex(new List<AttributeDefinition>
                {
                    table.First(x => x.Name == "Producer")
                }, 0);

                relationManager.CreateTable(table);
            }
        }

        public static Condition BooleanExpressionToCondition(TableDefinition tableDefinition, BooleanExpressionASTNode expr)
        {
            if (expr is AndASTNode andNode)
            {
                Condition c = new AndCondition()
                {
                    Left = BooleanExpressionToCondition(tableDefinition, andNode.Left),
                    Right = BooleanExpressionToCondition(tableDefinition, andNode.Right),
                };

                return c;
            }
            else if (expr is OrASTNode orNode)
            {
                Condition c = new OrCondition()
                {
                    Left = BooleanExpressionToCondition(tableDefinition, orNode.Left),
                    Right = BooleanExpressionToCondition(tableDefinition, orNode.Right),
                };

                return c;
            }
            else if (expr is RelOpASTNode relopNode
                && relopNode.Left is IdentifierASTNode idAstNode
                && relopNode.Right is StringASTNode stringAstNode)
            {
                return new LeafCondition
                {
                    Column = tableDefinition.First(x => x.Name.ToLower() == idAstNode.Identifier.ToLower()),
                    Operation = relopNode.RelationOperator,
                    Value = stringAstNode.Value
                };
            }

            return null;
        }


        private static void WriteProducers(Table table)
        {
            table.Insert(new object[] { 1, "Intel" });
            table.Insert(new object[] { 2, "AMD" });

            table.Write();
        }

        private static void ReadProducers(Table table)
        {
        }

        private static void WriteProducts(Table table)
        {
            table.Insert(1, new object[] { 1, 1994, "Intel" });
            table.Insert(2, new object[] { 2, 2010, "AMD" });
            table.Insert(4, new object[] { 4, 2020, "AMD" });
            table.Insert(3, new object[] { 3, 2015, "Intel" });

            table.Write();

            //string s = root.ToDot();
        }

        private static void ReadProducts(Table table)
        {
            Pointer dataPointer1 = table.Find(3);

            Set set = table.All();

            CustomTuple record = set.Find(dataPointer1.Index);
        }
    }
}

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

            Write(relationManager.GetTable("Products"));
            Read(relationManager.GetTable("Products"));


            string query = File.ReadAllText("query.txt");

            Command command = ParseCommand(relationManager, query);

            QueryPlan plan = new QueryPlan(command);

            List<CustomTuple> result = plan.Execute();
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
                    Condition = BooleanExpressionToCondition(table.TableDefinition, selectCommandAST.Condition)
                };

                return selectCommand;
            }

            return null;
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
                table.Add(new AttributeDefinition() { Name = "Maker", Type = ValueType.String });
                table.AddClusteredIndex(new List<AttributeDefinition>
                {
                    table.First(x => x.Name == "Id" )
                });

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

        //private static Set IndexSearch(Table table, BooleanExpressionASTNode expression)
        //{
        //    IBPlusTreeNode tree = table.RootBTreeNode;

        //    return IndexSearch(table, new Set(table.TableDefinition), tree);
        //}

        //private static Set IndexSearch(Table table, Set result, IBPlusTreeNode node)
        //{
        //    foreach (BPlusTreeNodeValue treeNodeValue in node.Values)
        //    {
        //        if (treeNodeValue.LeftPointer != null)
        //        {
        //            IndexSearch(table, result, node.ReadNode(treeNodeValue.LeftPointer.Short));
        //        }

        //        if (treeNodeValue.Pointer != null)
        //        {
        //            DataBlock block = table.StorageFile.ReadBlock(treeNodeValue.Pointer.PageNumber) as DataBlock;

        //            Set set = block.GetSet();

        //            CustomTuple record = set.Find(treeNodeValue.Pointer.Index);
        //            result.Add(record);
        //        }

        //        if (treeNodeValue.RightPointer != null)
        //        {
        //            IndexSearch(table, result, node.ReadNode(treeNodeValue.RightPointer.Short));
        //        }
        //    }

        //    return result;
        //}

        private static void Write(Table table)
        {
            table.Insert(1, new object[] { 1, 1994, "Intel" });
            table.Insert(2, new object[] { 2, 2010, "AMD" });
            table.Insert(4, new object[] { 4, 2020, "AMD" });
            table.Insert(3, new object[] { 3, 2015, "Intel" });

            table.Write();

            //string s = root.ToDot();
        }

        private static void Read(Table table)
        {
            Pointer dataPointer1 = table.Find(3);

            Block block = table.StorageFile.ReadBlock(table.TableDefinition, dataPointer1) as Block;

            Set set = block.GetSet();

            CustomTuple record = set.Find(dataPointer1.Index);
        }
    }
}

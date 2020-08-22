using Compiler.LexicalAnalyer;
using Compiler.Parser;
using Compiler.Parser.SyntaxTreeNodes;
using DatabaseEngine.Commands;
using DatabaseEngine.Operations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DatabaseEngine
{
    public class Program
    {

        public static List<Relation> Relations = new List<Relation>();
        public static List<Table> Tables = new List<Table>();

        public static string StorageFilePath = $"{Directory.GetCurrentDirectory()}\\data.storage";

        unsafe static void Main(string[] args)
        {
            File.Delete(StorageFilePath);


            // todo: store relations on disk

            TableDefinition ProductsTableDefinition = new TableDefinition()
            {
                Name = "Products",
                Id = 1
            };

            ProductsTableDefinition.Add(new AttributeDefinition() { Name = "Id", Type = ValueType.Integer });
            ProductsTableDefinition.Add(new AttributeDefinition() { Name = "BuildYear", Type = ValueType.Integer });
            ProductsTableDefinition.Add(new AttributeDefinition() { Name = "Maker", Type = ValueType.String });
            ProductsTableDefinition.AddClusteredIndex(new List<AttributeDefinition> 
            {
                ProductsTableDefinition.First(x => x.Name == "Id" )
            });
            Relations.Add(ProductsTableDefinition);

            StorageFile storageFile = new StorageFile(StorageFilePath);

            foreach(TableDefinition tableDefinition in Relations.Where(x => x is TableDefinition).ToList())
            {
                Table table = new Table(storageFile, tableDefinition);
                Tables.Add(table);
            }

            Write(Tables[0]);


            Tables.Clear();
            storageFile = new StorageFile(StorageFilePath);

            foreach (TableDefinition tableDefinition in Relations.Where(x => x is TableDefinition).ToList())
            {
                Table table = new Table(storageFile, tableDefinition);
                Tables.Add(table);
            }

            Read(Tables[0]);


            string input = File.ReadAllText("query.txt");

            LexicalAnalyzer analyzer = new LexicalAnalyzer(LexicalLanguage.GetLanguage(), input);
            BottomUpParser parser = new BottomUpParser(analyzer);

            parser.Parse();
            parser.OutputDebugFiles();

            SyntaxTreeNode command = parser.TopLevelAST;

            if (command is SelectASTNode selectCommandAST)
            {
                Table table = Tables.FirstOrDefault(x => x.TableDefinition.Name.ToLower() == selectCommandAST.From.Identifier.Identifier.ToLower());

                //Set result = IndexSearch(table, null);

                SelectCommand selectCommand = new SelectCommand
                {
                    Table = table,
                    Condition = BooleanExpressionToCondition(table.TableDefinition, selectCommandAST.Condition)
                };

                QueryPlan plan = new QueryPlan(selectCommand);

                List<CustomTuple> result = plan.Execute();
            }
        }

        public static Relation GetOrCreateIndexRelation(ValueType type)
        {
            int id = type == ValueType.Integer ? 2 : 3;

            if (Relations.Any(x => x.Id == id))
            {
                return Relations.First(x => x.Id == id);
            }

            Relation indexRelation = new Relation() { Name = "IndexRelation", Id = id };
            indexRelation.Add(new AttributeDefinition() { Name = "Value", Type = type });
            indexRelation.Add(new AttributeDefinition() { Name = "LeftPointer", Type = ValueType.Integer });
            indexRelation.Add(new AttributeDefinition() { Name = "ValuePointer", Type = ValueType.Integer });
            indexRelation.Add(new AttributeDefinition() { Name = "RightPointer", Type = ValueType.Integer });
            Relations.Add(indexRelation);

            return indexRelation;
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
            StorageFile storageFile = table.StorageFile;

            table.Insert(1, new object[] { 1, 1994, "Intel" });
            table.Insert(2, new object[] { 2, 2010, "AMD" });
            table.Insert(4, new object[] { 4, 2020, "AMD" });
            table.Insert(3, new object[] { 3, 2015, "Intel" });

            table.Write();

            //string s = root.ToDot();
        }

        private static void Read(Table table)
        {
            Pointer dataPointer1 = table.RootBTreeNode.Find(3);

            DataBlock block = table.StorageFile.ReadBlock(dataPointer1.PageNumber) as DataBlock;

            Set set = block.GetSet();

            CustomTuple record = set.Find(dataPointer1.Index);
        }
    }

    public enum NativeFileAccess : uint
    {
        GENERIC_READ = 0x80000000u,
        GENERIC_WRITE = 0x40000000u
    }

    public enum NativeShareMode : uint
    {
        FILE_SHARE_READ = 0x1,
        FILE_SHARE_WRITE = 0x2u
    }

    public enum NativeCreationDeposition : uint
    {
        OPEN_EXISTING = 0x3u,
        OPEN_ALWAYS = 0x4u
    }

    public enum FileAttribute : uint
    {
        NORMAL = 0x80u
    }
}

using System.Collections.Generic;
using System.IO;

namespace DatabaseEngine
{
    public class Program
    {

        public static List<TableDefinition> TableDefinitions = new List<TableDefinition>();
        public static List<Table> Tables = new List<Table>();

        public static string StorageFilePath = $"{Directory.GetCurrentDirectory()}\\data.storage";
        public static TableDefinition ProductsTableDefinition;

        unsafe static void Main(string[] args)
        {
            File.Delete(StorageFilePath);

            ProductsTableDefinition = new TableDefinition()
            {
                Name = "Product",
                Id = 1,
                ClusteredIndex = "Id"
            };
            ProductsTableDefinition.Add(new AttributeDefinition() { Name = "Id", Type = ValueType.Integer });
            ProductsTableDefinition.Add(new AttributeDefinition() { Name = "BuildYear", Type = ValueType.Integer });
            ProductsTableDefinition.Add(new AttributeDefinition() { Name = "Maker", Type = ValueType.String });
            TableDefinitions.Add(ProductsTableDefinition);

            StorageFile storageFile = new StorageFile(StorageFilePath);

            foreach(TableDefinition tableDefinition in TableDefinitions)
            {
                Table table = new Table(storageFile, tableDefinition);
                Tables.Add(table);
            }

            Write(Tables[0]);


            Tables.Clear();
            storageFile = new StorageFile(StorageFilePath);

            foreach (TableDefinition tableDefinition in TableDefinitions)
            {
                Table table = new Table(storageFile, tableDefinition);
                Tables.Add(table);
            }

            Read(Tables[0]);
        }

        private static void Write(Table table)
        {
            StorageFile storageFile = table.StorageFile;

            table.Insert(1, new object[] { 1, 1994, "Intel" });
            table.Insert(2, new object[] { 2, 2010, "AMD" });
            table.Insert(4, new object[] { 4, 2020, "AMD" });
            table.Insert(3, new object[] { 3, 2015, "Intel" });

            IEnumerable<BPlusTreeNode> dirtyNodes = table.RootBTreeNode.GetDirty();

            storageFile.Write();

            foreach (BPlusTreeNode dirtyNode in dirtyNodes)
            {
                dirtyNode.Write();
            }

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

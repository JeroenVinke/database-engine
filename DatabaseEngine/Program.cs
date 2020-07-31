using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace DatabaseEngine
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            uint lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            uint hTemplateFile
        );


        public delegate void WriteFileCompletionDelegate(UInt32 dwErrorCode,
          UInt32 dwNumberOfBytesTransfered, ref NativeOverlapped lpOverlapped);

        public static string StorageFile;

        public static List<TableDefinition> Tables = new List<TableDefinition>();


        unsafe static void Main(string[] args)
        {
            StorageFile = $"{Directory.GetCurrentDirectory()}\\data.storage";

            IntPtr fileHandle = OpenOrCreateFile();
            Block storageBlock = CreateStorageBlock();

            //Block indexBlock = CreateRootBlock();
            //Block branchBlock = CreateBranchBlock();

            ;
        }

        //private static Block CreateBranchBlock()
        //{
        //    Block branchBlock = new Block();
        //    branchBlock.Type = BlockType.Branch;

        //    branchBlock.AddRecord(new IndexRecord { });

        //    return branchBlock;
        //}

        //private static Block CreateRootBlock()
        //{
        //    Block branchBlock = new Block();
        //    branchBlock.Type = BlockType.Root;

        //    branchBlock.AddRecord(new IndexRecord { });

        //    return branchBlock;
        //}

        private static Block CreateStorageBlock()
        {
            TableDefinition productsTable = new TableDefinition()
            {
                Name = "Product",
                Id = 1
            };
            productsTable.Add(new AttributeDefinition() { Name = "BuildYear", Type = ValueType.Integer });
            productsTable.Add(new AttributeDefinition() { Name = "Maker", Type = ValueType.String });
            Tables.Add(productsTable);

            Set products = new Set(productsTable);
            products.Add(new object[] { 1994, "Intel" });
            products.Add(new object[] { 2010, "AMD" });

            return Block.CreateDataBlock(products);
        }

        //private static void Save(Set products)
        //{
        //IntPtr fileHandle = OpenOrCreateFile();

        //Block block = Block.CreateFromSet(products);
        //block.Write(fileHandle);

        //fileHandle = OpenOrCreateFile();
        //Block readBlock = Block.ReadBlock(fileHandle, 0, (TableDefinition)products.Relation);
        //Set readProducts = readBlock.GetSet();

        //int count = readProducts.Count();
        //}

        private static IntPtr OpenOrCreateFile()
        {
            IntPtr fileHandle = CreateFile(StorageFile,
                      (uint)NativeFileAccess.GENERIC_READ | (uint)NativeFileAccess.GENERIC_WRITE,
                      (uint)NativeShareMode.FILE_SHARE_READ | (uint)NativeShareMode.FILE_SHARE_WRITE,
                      0,
                      (uint)NativeCreationDeposition.OPEN_ALWAYS,
                      (uint)FileAttribute.NORMAL,
                      0);

            return fileHandle;
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

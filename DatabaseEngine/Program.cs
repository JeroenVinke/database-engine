﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace DatabaseEngine
{
    public class Program
    {

        public static List<TableDefinition> Tables = new List<TableDefinition>();


        unsafe static void Main(string[] args)
        {
            StorageFile storageFile = new StorageFile($"{Directory.GetCurrentDirectory()}\\data.storage");

            Pointer dataBlock1 = storageFile.GetFreeBlock();

            BPlusTreeNode root = new BPlusTreeNode(storageFile.GetFreeBlock());
            root.IsLeaf = true;
            root.StorageFile = storageFile;
            root.AddValue(10, new Pointer(dataBlock1.PageNumber, 1));
            root.AddValue(20, new Pointer(dataBlock1.PageNumber, 2));
            root.AddValue(30, new Pointer(dataBlock1.PageNumber, 3));
            root.AddValue(40, new Pointer(dataBlock1.PageNumber, 4));
            root.AddValue(50, new Pointer(dataBlock1.PageNumber, 5));
            root.AddValue(60, new Pointer(dataBlock1.PageNumber, 6));
            root.AddValue(70, new Pointer(dataBlock1.PageNumber, 7));
            root.AddValue(80, new Pointer(dataBlock1.PageNumber, 8));
            root.AddValue(90, new Pointer(dataBlock1.PageNumber, 9));
            root.AddValue(100, new Pointer(dataBlock1.PageNumber, 10));

            Pointer dataPointer = root.Find(40);

            string s = root.ToDot();


            //Block storageBlock = CreateStorageBlock();
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

﻿namespace DatabaseEngine
{
    public class Table
    {
        public BPlusTreeNode RootBTreeNode { get; set; }
        public StorageFile StorageFile { get; set; }
        public TableDefinition TableDefinition { get; set; }

        public Table(StorageFile storageFile, TableDefinition tableDefinition)
        {
            StorageFile = storageFile;
            TableDefinition = tableDefinition;

            if (!string.IsNullOrEmpty(tableDefinition.ClusteredIndex))
            {
                RootBTreeNode = new BPlusTreeNode(storageFile, new Pointer(StorageFile.RootBlock, 0));
                RootBTreeNode.IsRoot = true;
                RootBTreeNode.ReadNode();
            }
        }

        public void Insert(int key, object[] entries)
        {
            Pointer spot = RootBTreeNode.Find(key, false);

            DataBlock dataBlock;

            if (spot == null)
            {
                Pointer freeBlock = StorageFile.GetFreeBlock();
                dataBlock = new DataBlock();
                dataBlock.Relation = TableDefinition;
                spot = freeBlock;
            }
            else
            {
                dataBlock = StorageFile.ReadBlock(spot.PageNumber) as DataBlock;
            }

            CustomTuple tuple = new CustomTuple(TableDefinition).WithEntries(entries);
            // if block has room, else create new block
            int index = dataBlock.AddRecord(tuple.ToRecord());

            RootBTreeNode.AddValue(key, new Pointer(spot.PageNumber, index));

            StorageFile.WriteBlock(spot.PageNumber, dataBlock.ToBytes());
            // write;
        }
    }
}

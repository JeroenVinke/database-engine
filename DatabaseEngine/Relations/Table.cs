using System;
using System.Linq;

namespace DatabaseEngine
{
    public class Table
    {
        public IBPlusTreeNode RootBTreeNode { get; set; }
        public StorageFile StorageFile { get; set; }
        public TableDefinition TableDefinition { get; set; }

        public Table(StorageFile storageFile, TableDefinition tableDefinition)
        {
            StorageFile = storageFile;
            TableDefinition = tableDefinition;

            if (tableDefinition.HasClusteredIndex())
            {
                Index index = tableDefinition.GetClusteredIndex();

                if (index.Columns.Any(x => x.Type == ValueType.String))
                {
                    RootBTreeNode = new BPlusTreeNode<string>(Program.GetOrCreateIndexRelation(ValueType.String), storageFile, new Pointer(StorageFile.RootBlock, 0));
                }
                else if(index.Columns.All(x => x.Type == ValueType.Integer))
                {
                    RootBTreeNode = new BPlusTreeNode<int>(Program.GetOrCreateIndexRelation(ValueType.Integer), storageFile, new Pointer(StorageFile.RootBlock, 0));
                }

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

        internal void Write()
        {
            StorageFile.Write();
            RootBTreeNode.WriteTree();
        }
    }
}

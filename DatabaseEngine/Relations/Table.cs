using DatabaseEngine.Relations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class Table
    {
        private IBPlusTreeNode _rootBTreeNode;
        private Block _rootBlock;
        public StorageFile StorageFile { get; set; }
        public TableDefinition TableDefinition { get; set; }
        private RelationManager _relationManager;

        public Table(RelationManager relationManager, StorageFile storageFile, TableDefinition tableDefinition, Pointer rootBlock)
        {
            StorageFile = storageFile;
            TableDefinition = tableDefinition;
            _relationManager = relationManager;

            if (tableDefinition.HasClusteredIndex())
            {
                Index index = tableDefinition.GetClusteredIndex();

                if (index.Columns.Any(x => x.Type == ValueType.String))
                {
                    _rootBTreeNode = new BPlusTreeNode<string>(_relationManager.GetRelation(Constants.StringIndexRelationId), storageFile, rootBlock);
                }
                else if(index.Columns.All(x => x.Type == ValueType.Integer))
                {
                    _rootBTreeNode = new BPlusTreeNode<int>(_relationManager.GetRelation(Constants.IntIndexRelationId), storageFile, rootBlock);
                }

                _rootBTreeNode.IsRoot = true;
                _rootBTreeNode.ReadNode();
            }
            else
            {
                _rootBlock = storageFile.ReadBlock(tableDefinition, rootBlock);
            }
        }

        public void Insert(int key, object[] entries)
        {
            Pointer spot = _rootBTreeNode.Find(key, false);

            Block dataBlock;

            if (spot == null)
            {
                Pointer freeBlock = StorageFile.GetFreeBlock();
                dataBlock = new Block(TableDefinition);
                spot = freeBlock;
            }
            else
            {
                dataBlock = StorageFile.ReadBlock(TableDefinition, spot) as Block;
            }

            CustomTuple tuple = new CustomTuple(TableDefinition).WithEntries(entries);
            // if block has room, else create new block
            int index = dataBlock.AddRecord(tuple.ToRecord());

            _rootBTreeNode.AddValue(key, new Pointer(spot.PageNumber, index));

            StorageFile.WriteBlock(spot.PageNumber, dataBlock.ToBytes());
        }

        public void Insert(object[] entries)
        {
            CustomTuple tuple = new CustomTuple(TableDefinition).WithEntries(entries);
            _rootBlock.AddRecord(tuple.ToRecord());

            StorageFile.WriteBlock(_rootBlock.Page.PageNumber, _rootBlock.ToBytes());
        }

        internal void Write()
        {
            StorageFile.Write();
            _rootBTreeNode?.WriteTree();
        }

        public Set All()
        {
            Block block = StorageFile.ReadBlock(TableDefinition, _rootBlock.Page);
            Set set = block.GetSet();

            return set;
        }

        public Pointer Find(object id)
        {
            return _rootBTreeNode.Find(id);
        }
    }
}

using Compiler.Parser.SyntaxTreeNodes;
using DatabaseEngine.Relations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class Table
    {
        public Block RootBlock { get; set; }
        public StorageFile StorageFile { get; set; }
        public TableDefinition TableDefinition { get; set; }
        private RelationManager _relationManager;
        private Dictionary<Index, IBPlusTreeNode> _indexesWithTrees = new Dictionary<Index, IBPlusTreeNode>();

        public Table(RelationManager relationManager, StorageFile storageFile, TableDefinition tableDefinition, Pointer rootBlock)
        {
            StorageFile = storageFile;
            TableDefinition = tableDefinition;
            _relationManager = relationManager;

            if (tableDefinition.HasClusteredIndex())
            {
                Index index = tableDefinition.GetClusteredIndex();
                _indexesWithTrees.Add(index, GetBTreeForIndex(rootBlock, index));
            }
            else
            {
                RootBlock = storageFile.ReadBlock(tableDefinition, rootBlock);
            }

            foreach(Index index in TableDefinition.NonClusteredIndexes())
            {
                _indexesWithTrees.Add(index, GetBTreeForIndex(index.RootPointer, index));
            }
        }

        public IBPlusTreeNode GetIndex(AttributeDefinition rightColumn)
        {
            return _indexesWithTrees.First(x => x.Key.Columns.First().Name == rightColumn.Name).Value;
        }

        private IBPlusTreeNode GetBTreeForIndex(Pointer rootBlock, Index index)
        {
            IBPlusTreeNode node = null;

            if (index.Columns.Any(x => x.Type == ValueType.String))
            {
                node = new BPlusTreeNode<string>(_relationManager.GetRelation(Constants.StringIndexRelationId), TableDefinition, StorageFile, rootBlock);
            }
            else if (index.Columns.All(x => x.Type == ValueType.Integer))
            {
                node = new BPlusTreeNode<int>(_relationManager.GetRelation(Constants.IntIndexRelationId), TableDefinition, StorageFile, rootBlock);
            }

            node.IsRoot = true;
            node.ReadNode();

            return node;
        }

        private IBPlusTreeNode ClusteredIndex => _indexesWithTrees.FirstOrDefault(x => x.Key.Clustered).Value;

        public void Insert(int key, object[] entries)
        {
            Pointer spot = ClusteredIndex.Find(key, false);

            Block dataBlock;

            if (spot == null)
            {
                Pointer freeBlock = StorageFile.GetFreeBlock();
                dataBlock = new Block(StorageFile, TableDefinition);
                spot = freeBlock;
            }
            else
            {
                dataBlock = StorageFile.ReadBlock(TableDefinition, spot) as Block;
            }

            CustomTuple tuple = new CustomTuple(TableDefinition).WithEntries(entries);
            // if block has room, else create new block
            int index = dataBlock.AddRecord(tuple.ToRecord());

            Pointer indexKey = new Pointer(spot.PageNumber, index);
            ClusteredIndex.AddValue(key, indexKey);

            StorageFile.WriteBlock(spot.PageNumber, dataBlock.ToBytes());

            foreach(KeyValuePair<Index, IBPlusTreeNode> indexTree in _indexesWithTrees)
            {
                object value = tuple.GetValueFor<object>(indexTree.Key.Columns[0].Name);
                indexTree.Value.AddValue(value, indexKey);
            }
        }

        public void Insert(object[] entries)
        {
            CustomTuple tuple = new CustomTuple(TableDefinition).WithEntries(entries);
            RootBlock.AddRecord(tuple.ToRecord());

            StorageFile.WriteBlock(RootBlock.Page.PageNumber, RootBlock.ToBytes());
        }

        internal void Write()
        {
            StorageFile.Write();
            ClusteredIndex?.WriteTree();
        }

        public Set All()
        {
            if (TableDefinition.HasClusteredIndex())
            {
                return ClusteredIndex.IndexSearch(null);
            }
            else
            {
                Block block = StorageFile.ReadBlock(TableDefinition, RootBlock.Page);
                Set set = block.GetSet();

                return set;
            }
        }

        public Pointer Find(object id)
        {
            return ClusteredIndex.Find(id);
        }
    }
}

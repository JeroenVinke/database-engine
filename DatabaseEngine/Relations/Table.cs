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

        public void Insert(object[] entries)
        {
            CustomTuple tuple = new CustomTuple(TableDefinition).WithEntries(entries);
            Block block;

            if (TableDefinition.HasClusteredIndex())
            {
                object key = tuple.GetValueFor<object>(TableDefinition.GetClusteredIndex().Columns.First().Name);
                Pointer spot = ClusteredIndex.Find(key, false);

                if (spot == null)
                {
                    Pointer freeBlock = StorageFile.GetFreeBlock();
                    block = new Block(StorageFile, TableDefinition);
                    block.Page = freeBlock;
                }
                else
                {
                    block = StorageFile.ReadBlock(TableDefinition, spot) as Block;
                }
            }
            else
            {
                block = RootBlock;
            }

            Pointer indexKey = block.AddRecord(tuple.ToRecord());

            foreach (KeyValuePair<Index, IBPlusTreeNode> indexTree in _indexesWithTrees)
            {
                object value = tuple.GetValueFor<object>(indexTree.Key.Columns[0].Name);
                indexTree.Value.AddValue(value, indexKey);
                indexTree.Value.WriteTree();
            }
        }
    }
}

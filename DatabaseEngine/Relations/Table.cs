using DatabaseEngine.Relations;
using DatabaseEngine.Storage;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class Table
    {
        public Block RootBlock { get; set; }
        public MemoryManager MemoryManager { get; set; }
        public TableDefinition TableDefinition { get; set; }
        private RelationManager _relationManager;
        private Dictionary<Index, IBPlusTreeNode> _indexesWithTrees = new Dictionary<Index, IBPlusTreeNode>();
        public bool BulkMode { get; set; }
        private IBPlusTreeNode ClusteredIndex => _indexesWithTrees.SingleOrDefault(x => x.Key.IsClustered).Value;
        private Dictionary<uint, Block> _bulkBlocks = new Dictionary<uint, Block>();

        public Table(RelationManager relationManager, MemoryManager memoryManager, TableDefinition tableDefinition, Pointer rootBlock)
        {
            MemoryManager = memoryManager;
            TableDefinition = tableDefinition;
            _relationManager = relationManager;

            RootBlock = memoryManager.Read(tableDefinition, rootBlock);

            if (tableDefinition.HasClusteredIndex())
            {
                Index index = tableDefinition.GetClusteredIndex();
                _indexesWithTrees.Add(index, GetBTreeForIndex(rootBlock, index));
            }

            foreach(Index index in TableDefinition.NonClusteredIndexes())
            {
                _indexesWithTrees.Add(index, GetBTreeForIndex(index.RootPointer, index));
            }
        }

        public IBPlusTreeNode GetIndex(string rightColumn)
        {
            return _indexesWithTrees.First(x => x.Key.Column.ToLower() == rightColumn.ToLower()).Value;
        }

        private IBPlusTreeNode GetBTreeForIndex(Pointer rootBlock, Index index)
        {
            IBPlusTreeNode node = null;

            ValueType valueType = TableDefinition.GetAttributeByName(index.Column).Type;

            if (valueType == ValueType.String)
            {
                node = new BPlusTreeNode<string>(_relationManager.GetRelation(Constants.StringIndexRelationId), TableDefinition, MemoryManager, rootBlock);
            }
            else if (valueType == ValueType.Integer || valueType == ValueType.UnsignedInteger)
            {
                node = new BPlusTreeNode<int>(_relationManager.GetRelation(Constants.IntIndexRelationId), TableDefinition, MemoryManager, rootBlock);
            }

            node.IsRoot = true;
            node.ReadNode();

            return node;
        }

        public void Insert(CustomTuple tuple)
        {
            Block block;

            if (TableDefinition.HasClusteredIndex())
            {
                object key = tuple.GetValueFor<object>(TableDefinition.GetClusteredIndex().Column);
                Pointer spot = ClusteredIndex.Find(key, false);

                if (spot == null)
                {
                    Pointer freeBlock = MemoryManager.GetFreeBlock();
                    block = new Block(MemoryManager, TableDefinition);
                    block.Page = freeBlock;

                    AddBulkBlock(block);
                }
                else
                {
                    if (BulkMode && _bulkBlocks.TryGetValue(spot.Short, out Block foundBlockInCache))
                    {
                        block = foundBlockInCache;
                    }
                    else
                    {
                        block = MemoryManager.Read(TableDefinition, spot) as Block;

                        AddBulkBlock(block);
                    }
                }
            }
            else
            {
                block = RootBlock;
            }

            (Pointer indexKey, Block targetBlock) = block.AddRecord(tuple.ToRecord());

            if (!BulkMode)
            {
                targetBlock.Write();
            }
            else
            {
                AddBulkBlock(targetBlock);
                AddBulkBlock(block);
            }

            foreach (KeyValuePair<Index, IBPlusTreeNode> indexTree in _indexesWithTrees)
            {
                object value = tuple.GetValueFor<object>(indexTree.Key.Column);
                indexTree.Value.AddValue(value, indexKey);

                string ss = ((BPlusTreeNode<int>)indexTree.Value).ToDot();
                if (!BulkMode)
                {
                    indexTree.Value.WriteTree();
                }
            }
        }

        public void Insert(object[] entries)
        {
            CustomTuple tuple = new CustomTuple(TableDefinition).WithEntries(entries);

            Insert(tuple);
        }

        private void AddBulkBlock(Block block)
        {
            if (BulkMode)
            {
                if (!_bulkBlocks.ContainsKey(block.Page.Short))
                {
                    _bulkBlocks.Add(block.Page.Short, block);
                }
            }
        }

        public void StartBulkMode()
        {
            if (BulkMode)
            {
                EndBulkMode();
            }
            BulkMode = true;
        }

        public void EndBulkMode()
        {
            foreach(Block bulkBlock in _bulkBlocks.Values)
            {
                bulkBlock.Write();
            }

            _bulkBlocks.Clear();

            foreach (KeyValuePair<Index, IBPlusTreeNode> indexTree in _indexesWithTrees)
            {
                indexTree.Value.WriteTree();
            }

            BulkMode = false;
        }
    }
}

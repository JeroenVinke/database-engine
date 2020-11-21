using DatabaseEngine.BTree;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class IndexSeekOperation : PhysicalOperation
    {
        public Table Table { get; set; }
        public Condition Condition { get; }

        public IBPlusTreeNode _index;
        public IBPlusTreeNode _currentNode;
        private ScanEnumerator _enumerator;

        public IndexSeekOperation(Table table, IBPlusTreeNode index, Condition condition)
            : base(new List<PhysicalOperation>())
        {
            Table = table;
            Condition = condition;
            _index = index;
        }

        public override void Prepare()
        {
            base.Prepare();

            _currentNode = _index.GetFirstLeaf();
            _enumerator = new ScanEnumerator(Table.TableDefinition, _index, Condition);
        }

        public override CustomTuple GetNext()
        {
            if (_enumerator.MoveNext())
            {
                Block block = Table.MemoryManager.Read(Table.TableDefinition, _enumerator.CurrentValue.Pointer);
                Record record = block.GetRecordForRowId(_enumerator.CurrentValue.Pointer.Index);

                return new CustomTuple(Table.TableDefinition).FromRecord(record);
            }

            return null;
        }
    }
}

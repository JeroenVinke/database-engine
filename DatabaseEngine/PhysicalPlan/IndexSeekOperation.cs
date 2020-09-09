using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class IndexSeekOperation : PhysicalOperation
    {
        public Table Table { get; set; }

        public IBPlusTreeNode _index;
        public IBPlusTreeNode _currentNode;
        private int _currentIndex = -1;

        public IndexSeekOperation(Table table, IBPlusTreeNode index)
            : base(new List<PhysicalOperation>())
        {
            Table = table;
            _index = index;
        }

        public override void Prepare()
        {
            base.Prepare();

            _currentNode = _index.GetFirstLeaf();
            _currentIndex = -1;
        }

        public override CustomTuple GetNext()
        {
            while (_currentNode != null)
            {
                int next = _currentIndex + 1;

                if (_currentNode.Values.Count > next)
                {

                    Block block = Table.MemoryManager.Read(Table.TableDefinition, _currentNode.Values[next].Pointer);
                    Record record = block.GetRecordForRowId(_currentNode.Values[next].Pointer.Index);

                    _currentIndex++;

                    return new CustomTuple(Table.TableDefinition).FromRecord(record);
                }
                else
                {
                    _currentIndex = -1;
                    _currentNode = _currentNode.Sibling;
                }
            }

            return null;
        }
    }
}

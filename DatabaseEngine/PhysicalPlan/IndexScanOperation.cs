using DatabaseEngine.LogicalPlan;
using System;

namespace DatabaseEngine.Operations
{
    public class IndexScanOperation : PhysicalOperation
    {
        public Table Table { get; set; }

        public IBPlusTreeNode _index;
        public IBPlusTreeNode _currentNode;
        private int _currentIndex = -1;

        public IndexScanOperation(LogicalElement logicalElement, Table table, IBPlusTreeNode index)
            : base(logicalElement)
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

        public override string ToString()
        {
            return typeof(IndexScanOperation).Name + "(" + Table.TableDefinition.Name + ")";
        }

        public override int EstimateIOCost()
        {
            int b = Program.StatisticsManager.B(Table.TableDefinition);

            if (b == 0)
            {
                return 0;
            }

            int t = Program.StatisticsManager.T(Table.TableDefinition);

            if (t == 0)
            {
                return 0;
            }

            int dataBlocks = (int)Math.Ceiling((double)t / (double)b);
            int indexBlocks = (int)Math.Ceiling((double)t / Program.StatisticsManager.GetBlockSize(_index.IndexRelation));

            return dataBlocks + indexBlocks;
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
                    _currentNode = _currentNode.RightSibling;
                }
            }

            return null;
        }
    }
}

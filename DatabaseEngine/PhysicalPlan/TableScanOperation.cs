using DatabaseEngine.LogicalPlan;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine.Operations
{
    public class TableScanOperation : PhysicalOperation
    {
        public Table Table { get; set; }
        private Block _currentBlock;
        private CustomTuple _currentTuple;
        private int _recordIndex = -1;

        public TableScanOperation(LogicalElement logicalElement, Table table)
            : base(logicalElement)
        {
            Table = table;
        }

        public override void Prepare()
        {
            base.Prepare();

            _currentBlock = Table.RootBlock;
            _recordIndex = -1;
        }

        public override string ToString()
        {
            return typeof(TableScanOperation).Name + "(" + Table.TableDefinition.Name + ")";
        }

        public override int EstimateIOCost()
        {
            return Program.StatisticsManager.B(Table.TableDefinition);
        }

        public override CustomTuple GetNext()
        {
            _recordIndex++;

            if (_currentBlock.GetSortedRecords().Count() <= _recordIndex)
            {
                if (_currentBlock.NextBlock != null)
                {

                    _currentBlock = _currentBlock.NextBlock;
                    _recordIndex = 0;
                }
                else
                {
                    return null;
                }
            }

            _currentTuple = new CustomTuple(Table.TableDefinition).FromRecord(_currentBlock.GetSortedRecords()[_recordIndex]);

            return _currentTuple;
        }
    }
}

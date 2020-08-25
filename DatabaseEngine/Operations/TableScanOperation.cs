using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class TableScanOperation : Operation
    {
        public Table Table { get; set; }
        private Block _currentBlock;
        private CustomTuple _currentTuple;
        private int _recordIndex = -1;

        public TableScanOperation(Table table)
            : base(new List<Operation>())
        {
            Table = table;
        }

        public override void Prepare()
        {
            base.Prepare();

            _currentBlock = Table.RootBlock;
            _recordIndex = -1;
        }

        public override CustomTuple GetNext()
        {
            _recordIndex++;

            if (_currentBlock.Records.Count <= _recordIndex)
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

            _currentTuple = new CustomTuple(Table.TableDefinition).FromRecord(_currentBlock.Records[_recordIndex]);

            return _currentTuple;
        }
    }
}

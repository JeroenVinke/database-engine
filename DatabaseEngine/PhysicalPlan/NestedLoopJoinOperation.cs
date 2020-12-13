using DatabaseEngine.LogicalPlan;

namespace DatabaseEngine.Operations
{
    public class NestedLoopJoinOperation : PhysicalOperation
    {
        private CustomTuple _leftTuple;
        private AttributeDefinition _leftJoinColumn;
        private AttributeDefinition _rightJoinColumn;

        public ReadLogicalElement LeftLogical { get; }
        public ReadLogicalElement RightLogical { get; }

        public NestedLoopJoinOperation(LogicalElement logicalElement, ReadLogicalElement left, ReadLogicalElement right, AttributeDefinition leftJoinColumn, AttributeDefinition rightJoinColumn)
            : base(logicalElement)
        {
            LeftLogical = left;
            RightLogical = right;
            _leftJoinColumn = leftJoinColumn;
            _rightJoinColumn = rightJoinColumn;
        }

        public override int EstimateIOCost()
        {
            //int leftSize = left.EstimateNumberOfRows();
            //int rightSize = right.EstimateNumberOfRows();

            //int x = _statisticsManager.GetSizeOfCondition((right.InputOperations.First() as TableScanOperation).Table.TableDefinition, (right as FilterOperation).Condition);

            return base.EstimateIOCost();
        }

        public override int EstimateCPUCost()
        {
            return LeftLogical.T() * RightLogical.T();
        }

        public override void Prepare()
        {
            base.Prepare();

            _leftTuple = Left.GetNext();
        }

        public override CustomTuple GetNext()
        {
            while (true)
            {
                CustomTuple rightTuple = Right.GetNext();

                if (rightTuple == null)
                {
                    Right.Unprepare();
                    _leftTuple = Left.GetNext();
                    Right.Prepare();
                }
                else if (_leftTuple != null && _leftTuple.Joins(rightTuple, _leftJoinColumn, _rightJoinColumn))
                {
                    CustomTuple result = _leftTuple;
                    _leftTuple = Left.GetNext();
                    return result.Merge(rightTuple);
                }

                if (_leftTuple == null)
                {
                    return null;
                }
            }
        }
    }
}

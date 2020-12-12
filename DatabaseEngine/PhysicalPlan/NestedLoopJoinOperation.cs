using DatabaseEngine.LogicalPlan;

namespace DatabaseEngine.Operations
{
    public class NestedLoopJoinOperation : PhysicalOperation
    {
        private CustomTuple _leftTuple;
        private AttributeDefinition _leftJoinColumn;
        private AttributeDefinition _rightJoinColumn;

        public NestedLoopJoinOperation(LogicalElement logicalElement, PhysicalOperation left, PhysicalOperation right, AttributeDefinition leftJoinColumn, AttributeDefinition rightJoinColumn)
            : base(logicalElement)
        {
            Left = left;
            Right = right;
            _leftJoinColumn = leftJoinColumn;
            _rightJoinColumn = rightJoinColumn;
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

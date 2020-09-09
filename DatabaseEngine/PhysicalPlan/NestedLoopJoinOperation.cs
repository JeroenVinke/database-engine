using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DatabaseEngine.Operations
{
    public class NestedLoopJoinOperation : PhysicalOperation
    {
        private PhysicalOperation _left;
        private PhysicalOperation _right;
        private CustomTuple _leftTuple;
        private AttributeDefinition _leftJoinColumn;
        private AttributeDefinition _rightJoinColumn;

        public NestedLoopJoinOperation(PhysicalOperation left, PhysicalOperation right, AttributeDefinition leftJoinColumn, AttributeDefinition rightJoinColumn)
            : base(new List<PhysicalOperation>() { left, right })
        {
            _left = left;
            _right = right;
            _leftJoinColumn = leftJoinColumn;
            _rightJoinColumn = rightJoinColumn;
        }

        public override void Prepare()
        {
            base.Prepare();

            _leftTuple = _left.GetNext();
        }

        public override CustomTuple GetNext()
        {
            while (true)
            {
                CustomTuple rightTuple = _right.GetNext();

                if (rightTuple == null)
                {
                    _right.Unprepare();
                    _leftTuple = _left.GetNext();
                    _right.Prepare();
                }
                else if (_leftTuple != null && _leftTuple.Joins(rightTuple, _leftJoinColumn, _rightJoinColumn))
                {
                    CustomTuple result = _leftTuple;
                    _leftTuple = _left.GetNext();
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

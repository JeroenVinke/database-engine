using DatabaseEngine.LogicalPlan;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class UnionOperator : PhysicalOperation
    {
        private bool _leftDone = false;

        public UnionOperator(LogicalElement logicalElement, PhysicalOperation left, PhysicalOperation right)
            : base(logicalElement)
        {
            Left = left;
            Right = right;
        }

        public override CustomTuple GetNext()
        {
            CustomTuple result;

            if (!_leftDone)
            {
                result = Left.GetNext();
                if (result == null)
                {
                    _leftDone = true;
                }
                else
                {
                    return result;
                }
            }
            
            result = Right.GetNext();
            if (result == null)
            {
                return null;
            }

            return result;
        }
    }
}

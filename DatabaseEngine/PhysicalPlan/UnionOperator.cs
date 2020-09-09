using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class UnionOperator : PhysicalOperation
    {
        private PhysicalOperation _left;
        private PhysicalOperation _right;
        private bool _leftDone = false;

        public UnionOperator(PhysicalOperation left, PhysicalOperation right)
            : base(new List<PhysicalOperation>() { left, right })
        {
            _left = left;
            _right = right;
        }

        public override CustomTuple GetNext()
        {
            CustomTuple result;

            if (!_leftDone)
            {
                result = _left.GetNext();
                if (result == null)
                {
                    _leftDone = true;
                }
                else
                {
                    return result;
                }
            }
            
            result = _right.GetNext();
            if (result == null)
            {
                return null;
            }

            return result;
        }
    }
}

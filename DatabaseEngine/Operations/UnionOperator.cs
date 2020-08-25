using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class UnionOperator : Operation
    {
        private Operation _left;
        private Operation _right;
        private bool _leftDone = false;

        public UnionOperator(Operation left, Operation right)
            : base(new List<Operation>() { left, right })
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

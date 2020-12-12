using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DatabaseEngine.Operations
{
    public class HashSetJoinOperation : PhysicalOperation
    {
        private PhysicalOperation _left;
        private PhysicalOperation _right;
        private AttributeDefinition _leftJoinColumn;
        private AttributeDefinition _rightJoinColumn;

        public HashSetJoinOperation(PhysicalOperation left, PhysicalOperation right, AttributeDefinition leftJoinColumn, AttributeDefinition rightJoinColumn)
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
        }

        public override CustomTuple GetNext()
        {
            return null;
        }
    }
}

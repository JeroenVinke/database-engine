using DatabaseEngine.LogicalPlan;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class HashSetJoinOperation : PhysicalOperation
    {
        private AttributeDefinition _leftJoinColumn;
        private AttributeDefinition _rightJoinColumn;

        public HashSetJoinOperation(LogicalElement logicalElement, PhysicalOperation left, PhysicalOperation right, AttributeDefinition leftJoinColumn, AttributeDefinition rightJoinColumn)
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
        }

        public override CustomTuple GetNext()
        {
            return null;
        }
    }
}

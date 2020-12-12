using DatabaseEngine.LogicalPlan;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class TopOperation : PhysicalOperation
    {
        private int? _amount;
        private int _returned = 0;

        public TopOperation(LogicalElement logicalElement, PhysicalOperation inputOperation, int? amount)
            :base (logicalElement)
        {
            Left = inputOperation;
            _amount = amount;
        }

        public override void Prepare()
        {
            base.Prepare();

            _returned = 0;
        }

        public override CustomTuple GetNext()
        {
            CustomTuple tuple = Left.GetNext();

            if (tuple != null && _returned < _amount)
            {
                _returned++;
                return tuple;
            }

            return null;
        }
    }
}

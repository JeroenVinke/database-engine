using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class TopOperation : PhysicalOperation
    {
        private PhysicalOperation _inputOperation;
        private int? _amount;
        private int _returned = 0;

        public TopOperation(PhysicalOperation inputOperation, int? amount)
            :base (new List<PhysicalOperation> { inputOperation })
        {
            _inputOperation = inputOperation;
            _amount = amount;
        }

        public override void Prepare()
        {
            base.Prepare();

            _returned = 0;
        }

        public override CustomTuple GetNext()
        {
            CustomTuple tuple = _inputOperation.GetNext();

            if (tuple != null && _returned < _amount)
            {
                _returned++;
                return tuple;
            }

            return null;
        }
    }
}

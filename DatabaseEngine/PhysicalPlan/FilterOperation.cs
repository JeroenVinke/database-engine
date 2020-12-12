using System;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class FilterOperation : PhysicalOperation
    {
        private PhysicalOperation _inputOperation;
        public Condition Condition { get; set; }

        public FilterOperation(PhysicalOperation inputOperation, Condition condition)
            :base (new List<PhysicalOperation> { inputOperation })
        {
            _inputOperation = inputOperation;
            Condition = condition;
        }

        public override CustomTuple GetNext()
        {
            CustomTuple tuple = _inputOperation.GetNext();

            if (tuple != null)
            {
                if (Condition.SatisfiesCondition(tuple))
                {
                    return tuple;
                }

                return GetNext();
            }

            return null;
        }
    }
}

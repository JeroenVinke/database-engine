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
                if (SatisfiesCondition(tuple, Condition))
                {
                    return tuple;
                }

                return GetNext();
            }

            return null;
        }

        public override int EstimateNumberOfRows()
        {
            return base.EstimateNumberOfRows();
        }

        private bool SatisfiesCondition(CustomTuple tuple, Condition condition)
        {
            if (condition == null)
            {
                return true;
            }

            if (condition is AndCondition andCondition)
            {
                return SatisfiesCondition(tuple, andCondition.Left) && SatisfiesCondition(tuple, andCondition.Right);
            }
            else if (condition is OrCondition orCondition)
            {
                return SatisfiesCondition(tuple, orCondition.Left) || SatisfiesCondition(tuple, orCondition.Right);
            }
            else if (condition is LeafCondition leafCondition)
            {
                CustomObject value = tuple.GetEntryFor(leafCondition.Column);

                switch (leafCondition.Operation)
                {
                    case Compiler.Common.RelOp.Equals:
                        return value.IsEqualTo(leafCondition.Value);
                    case Compiler.Common.RelOp.GreaterThan:
                        return value.IsGreaterThan(leafCondition.Value);
                }
            }

            return false;
        }
    }
}

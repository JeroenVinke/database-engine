using System;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class FilterOperation : Operation
    {
        private Operation _inputOperation;
        private Condition _condition;

        public FilterOperation(Operation inputOperation, Condition condition)
            :base (new List<Operation> { inputOperation })
        {
            _inputOperation = inputOperation;
            _condition = condition;
        }

        public override CustomTuple GetNext()
        {
            CustomTuple tuple = _inputOperation.GetNext();

            if (tuple != null)
            {
                if (SatisfiesCondition(tuple, _condition))
                {
                    return tuple;
                }

                return GetNext();
            }

            return null;
        }

        private bool SatisfiesCondition(CustomTuple tuple, Condition condition)
        {
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
                CustomObject value = tuple.GetEntryFor(leafCondition.Column.Name);

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

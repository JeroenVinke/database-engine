﻿using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class TopOperation : Operation
    {
        private Operation _inputOperation;
        private int? _amount;
        private int _returned = 0;

        public TopOperation(Operation inputOperation, int? amount)
            :base (new List<Operation> { inputOperation })
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

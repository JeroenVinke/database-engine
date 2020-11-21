using System;

namespace DatabaseEngine.LogicalPlan
{
    public class SelectionElement : ReadLogicalElement
    {
        public Condition Condition { get; set; }

        public SelectionElement(ReadLogicalElement input, Condition condition)
            : base(input)
        {
            Condition = condition;
        }

        public override int T()
        {
            return GetSizeOfCondition(LeftChild, Condition);
        }

        public override double V(AttributeDefinition column)
        {
            return LeftChild.V(column);
        }

        public int GetSizeOfCondition(ReadLogicalElement input, Condition condition)
        {
            int sizeOfRelation = input.T();

            if (condition == null)
            {
                return sizeOfRelation;
            }

            if (condition is AndCondition andCondition)
            {
                return sizeOfRelation * ((GetSizeOfCondition(input, andCondition.Left) / sizeOfRelation) * (GetSizeOfCondition(input, andCondition.Right) / sizeOfRelation));
            }
            else if (condition is OrCondition orCondition)
            {
                int m1 = GetSizeOfCondition(input, orCondition.Left);
                int m2 = GetSizeOfCondition(input, orCondition.Right);
                return sizeOfRelation * (1 - ((1 - (m1 / sizeOfRelation)) * ((m2 / sizeOfRelation))));
            }
            else if (condition is LeafCondition leafCondition)
            {
                switch (leafCondition.Operation)
                {
                    case Compiler.Common.RelOp.Equals:
                        return (int)Math.Round(sizeOfRelation * ((double)1 / (double)input.V(leafCondition.Column)));
                    case Compiler.Common.RelOp.GreaterOrEqualThan:
                    case Compiler.Common.RelOp.GreaterThan:
                    case Compiler.Common.RelOp.LessOrEqualThan:
                    case Compiler.Common.RelOp.LessThan:
                    case Compiler.Common.RelOp.NotEquals:
                        return sizeOfRelation;
                }
            }

            return -1;
        }


        public override string Stringify()
        {
            return "SELECTION(" + Condition.ToString() + ")";
        }
    }
}

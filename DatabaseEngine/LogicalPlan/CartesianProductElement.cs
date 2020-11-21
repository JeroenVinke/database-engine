using System;

namespace DatabaseEngine.LogicalPlan
{
    public class CartesianProductElement : ReadLogicalElement
    {
        public AttributeDefinition LeftJoinColumn { get; set; }
        public AttributeDefinition RightJoinColumn { get; set; }

        public CartesianProductElement(ReadLogicalElement left, ReadLogicalElement right, AttributeDefinition leftJoinColumn, AttributeDefinition rightJoinColumn)
            : base(left, right)
        {
            LeftJoinColumn = leftJoinColumn;
            RightJoinColumn = rightJoinColumn;
        }

        public override double V(AttributeDefinition column)
        {
            throw new System.NotImplementedException();
        }

        public override int T()
        {
            int r = (int)Math.Round(Math.Max(LeftChild.V(LeftJoinColumn), RightChild.V(RightJoinColumn)));

            if (r == 0)
            {
                return 0;
            }

            return (int)Math.Round((double)(LeftChild.T() * RightChild.T()) / r);
        }

        public override string Stringify()
        {
            return "CARTESIAN PRODUCT (" + LeftJoinColumn.Name + " - " + RightJoinColumn.Name + ")";
        }
    }
}

namespace DatabaseEngine.LogicalPlan
{
    public class CartesianProductElement : LogicalElement
    {
        public AttributeDefinition LeftJoinColumn { get; set; }
        public AttributeDefinition RightJoinColumn { get; set; }

        public CartesianProductElement(LogicalElement left, LogicalElement right, AttributeDefinition leftJoinColumn, AttributeDefinition rightJoinColumn)
            : base(left, right)
        {
            LeftJoinColumn = leftJoinColumn;
            RightJoinColumn = rightJoinColumn;
        }

        public override string Stringify()
        {
            return "CARTESIAN PRODUCT (" + LeftJoinColumn.Name + " - " + RightJoinColumn.Name + ")";
        }
    }
}

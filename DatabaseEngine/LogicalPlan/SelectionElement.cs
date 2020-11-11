namespace DatabaseEngine.LogicalPlan
{
    public class SelectionElement : LogicalElement
    {
        public Relation Relation { get; set; }

        public Condition Condition { get; set; }

        public SelectionElement(LogicalElement input, Condition condition, Relation relation)
            : base(input)
        {
            Condition = condition;
            Relation = relation;
        }

        public override string Stringify()
        {
            return "SELECTION(" + Condition.ToString() + ")";
        }
    }
}

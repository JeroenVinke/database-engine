namespace DatabaseEngine.LogicalPlan
{
    public class FilterElement : LogicalElement
    {
        public Condition Condition { get; set; }

        public FilterElement(LogicalElement input, Condition condition)
            : base(input)
        {
            Condition = condition;
        }

        public override string Stringify()
        {
            return "FILTER(" + Condition.ToString() + ")";
        }
    }
}

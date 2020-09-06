namespace DatabaseEngine.LogicalPlan
{
    public class FilterElement : LogicalElement
    {
        private Condition _condition;

        public FilterElement(LogicalElement input, Condition condition)
            : base(input)
        {
            _condition = condition;
        }

        public override string ToString()
        {
            return "FILTER(" + _condition.ToString() + ")";
        }
    }
}

using DatabaseEngine.LogicalPlan;

namespace DatabaseEngine.Operations
{
    public class FilterOperation : PhysicalOperation
    {
        public Condition Condition { get; set; }

        public FilterOperation(LogicalElement logicalElement, PhysicalOperation inputOperation, Condition condition)
            :base (logicalElement)
        {
            Left = inputOperation;
            Condition = condition;
        }

        public override CustomTuple GetNext()
        {
            CustomTuple tuple = Left.GetNext();

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

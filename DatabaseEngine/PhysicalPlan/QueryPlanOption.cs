using DatabaseEngine.LogicalPlan;
using DatabaseEngine.Operations;

namespace DatabaseEngine.PhysicalPlan
{
    public class QueryPlanBBNode : BBNode
    {
        public PhysicalOperation PhysicalOperation { get; set; }
        public LogicalElement LogicalElement { get; set; }
        public bool IsRoot => PhysicalOperation == null;

        public QueryPlanBBNode(LogicalElement logicalElement, PhysicalOperation physicalOperation)
        {
            LogicalElement = logicalElement;
            PhysicalOperation = physicalOperation;
        }
    }
}

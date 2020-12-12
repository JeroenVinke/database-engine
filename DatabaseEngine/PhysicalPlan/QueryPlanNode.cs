using DatabaseEngine.LogicalPlan;
using DatabaseEngine.Operations;
using System.Collections.Generic;

namespace DatabaseEngine.PhysicalPlan
{
    public class QueryPlanNode
    {
        public PhysicalOperation PhysicalOperation { get; set; }
        public LogicalElement LogicalElement { get; set; }
        public List<QueryPlanNodeEdge> Edges { get; set; } = new List<QueryPlanNodeEdge>();

        public QueryPlanNode(LogicalElement logicalElement, PhysicalOperation physicalOperation)
        {
            LogicalElement = logicalElement;
            PhysicalOperation = physicalOperation;
        }
    }
}

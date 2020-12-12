using DatabaseEngine.PhysicalPlan;

namespace DatabaseEngine
{
    public class QueryPlanNodeEdge
    {
        public QueryPlanNode From { get; set; }
        public QueryPlanNode To { get; set; }
        public int Cost { get; internal set; }
    }
}

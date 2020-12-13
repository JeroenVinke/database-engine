using DatabaseEngine.LogicalPlan;
using DatabaseEngine.Operations;
using System.Collections.Generic;

namespace DatabaseEngine.PhysicalPlan
{
    public class QueryPlanNode
    {
        public int Id { get; set; }
        public static int MaxId = 0;
        public PhysicalOperation PhysicalOperation { get; set; }
        public LogicalElement LogicalElement { get; set; }
        public List<QueryPlanNodeEdge> Edges { get; set; } = new List<QueryPlanNodeEdge>();

        public QueryPlanNode(LogicalElement logicalElement, PhysicalOperation physicalOperation)
        {
            LogicalElement = logicalElement;
            PhysicalOperation = physicalOperation;
            Id = MaxId++;
        }


        public string ToDot()
        {
            string dot = "";

            dot += Id + "[label=\"" + (PhysicalOperation != null ? PhysicalOperation.ToString() : "root") + "\"]\r";

            foreach(QueryPlanNodeEdge edge in Edges)
            {
                dot += edge.To.ToDot();
                dot += Id + " -> " + edge.To.Id + "[label=" + edge.Cost + "]\r";
            }

            return dot;
        }
    }
}

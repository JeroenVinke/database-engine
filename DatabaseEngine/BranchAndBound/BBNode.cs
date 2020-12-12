using DatabaseEngine.LogicalPlan;
using DatabaseEngine.Operations;
using DatabaseEngine.PhysicalPlan;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseEngine
{
    public abstract class BBGraph
    {
        public BBNode Root { get; set; }

        public BBGraph()
        {
            Root = new BBNode();
        }

        public abstract void Expand(BBNode node);
    }

    public class BBNode
    {
        public List<BBEdge> Edges = new List<BBEdge>();

        public BBNode()
        {
        }

        public void Expand()
        {

        }
    }

    public class StartingBBNode : QueryPlanBBNode
    {
        public StartingBBNode(LogicalElement logicalElement, PhysicalOperation physicalOperation) : base(logicalElement, physicalOperation)
        {
        }
    }

    public class EndBBNode : QueryPlanBBNode
    {
        public EndBBNode(LogicalElement logicalElement, PhysicalOperation physicalOperation) : base(logicalElement, physicalOperation)
        {
        }
    }

    public class BBEdge
    {
        public BBNode From { get; set; }
        public BBNode To { get; set; }
        public int Cost { get; internal set; }
    }
}

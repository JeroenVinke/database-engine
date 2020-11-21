using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine.LogicalPlan
{
    public class ProjectionElement : ReadLogicalElement
    {
        public List<ProjectionColumn> Columns { get; set; }

        public ProjectionElement(ReadLogicalElement input, List<ProjectionColumn> columns)
            : base(input)
        {
            Columns = columns;
        }

        public override int T()
        {
            return LeftChild.T();
        }

        public override double V(AttributeDefinition column)
        {
            return LeftChild.V(column);
        }

        public override string Stringify()
        {
            return "PROJECT(" + string.Join(", ", Columns.Select(x => x.Relation.Name + "." + x.AttributeDefinition.Name)) + ")";
        }
    }
}

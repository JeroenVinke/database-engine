using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine.LogicalPlan
{
    public class ProjectionElement : LogicalElement
    {
        public List<ProjectionColumn> Columns { get; set; }

        public ProjectionElement(LogicalElement input, List<ProjectionColumn> columns)
            : base(input)
        {
            Columns = columns;
        }

        public override string Stringify()
        {
            return "PROJECT(" + string.Join(", ", Columns.Select(x => x.Relation.Name + "." + x.AttributeDefinition.Name)) + ")";
        }
    }
}

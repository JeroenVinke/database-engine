using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine.LogicalPlan
{
    public class ProjectionElement : LogicalElement
    {
        private List<AttributeDefinition> _columnsToSelect;

        public ProjectionElement(LogicalElement input, List<AttributeDefinition> columnsToSelect)
            : base(input)
        {
            _columnsToSelect = columnsToSelect;
        }

        public override string ToString()
        {
            return "PROJECT(" + string.Join(", ", _columnsToSelect.Select(x => x.Name)) + ")";
        }
    }
}

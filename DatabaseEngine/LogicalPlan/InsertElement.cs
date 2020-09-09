using System.Collections.Generic;

namespace DatabaseEngine.LogicalPlan
{
    public class InsertElement : LogicalElement
    {
        public TableDefinition TableDefinition { get; set; }

        public InsertElement(TableDefinition tableDefinition, LogicalElement child)
            : base(child)
        {
            TableDefinition = tableDefinition;
        }

        public override string Stringify()
        {
            return "INSERT";
        }
    }
}

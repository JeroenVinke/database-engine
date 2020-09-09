using System.Collections.Generic;

namespace DatabaseEngine
{
    public class Join
    {
        public TableDefinition LeftTable { get; set; }
        public TableDefinition RightTable { get; set; }
        public AttributeDefinition LeftColumn { get; set; }
        public AttributeDefinition RightColumn { get; set; }
    }
}
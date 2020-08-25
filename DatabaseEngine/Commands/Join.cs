using System.Collections.Generic;

namespace DatabaseEngine
{
    public class Join
    {
        public Table LeftTable { get; set; }
        public Table RightTable { get; set; }
        public AttributeDefinition LeftColumn { get; set; }
        public AttributeDefinition RightColumn { get; set; }
    }
}
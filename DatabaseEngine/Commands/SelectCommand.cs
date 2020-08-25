using System.Collections.Generic;

namespace DatabaseEngine.Commands
{
    public class SelectCommand : Command
    {
        public Table Table { get; set; }
        public List<AttributeDefinition> Columns { get; set; } = new List<AttributeDefinition>();
        public Condition Condition { get; set; }
        public Join Join { get; set; }
    }
}

using System.Collections.Generic;

namespace DatabaseEngine.Commands
{
    public class SelectCommand : Command
    {
        public List<Table> SelectList { get; set; }
        public List<AttributeDefinition> Columns { get; set; } = new List<AttributeDefinition>();
        public Condition Condition { get; set; }
        public Join Join { get; set; }
        public int? Top { get; set; }
    }
}

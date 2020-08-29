using System.Collections.Generic;

namespace DatabaseEngine.Commands
{
    public class InsertCommand : Command
    {
        public Table Table { get; set; }
        public List<object> Values { get; set; } = new List<object>();
    }
}

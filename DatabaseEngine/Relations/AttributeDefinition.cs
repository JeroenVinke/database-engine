using DatabaseEngine.Models;

namespace DatabaseEngine
{
    public class AttributeDefinition
    {
        [FromColumn("Name")]
        public string Name { get; set; }
        [FromColumn("Type")]
        public ValueType Type { get; set; }
        public Relation Relation { get; set; }
    }
}

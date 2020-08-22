using System.Collections.Generic;

namespace DatabaseEngine
{
    public class Relation : List<AttributeDefinition>
    {
        public int Id { get; internal set; }
        public string Name { get; set; }
    }
}

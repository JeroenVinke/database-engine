using System.Collections.Generic;

namespace DatabaseEngine
{
    public class Index
    {
        public List<AttributeDefinition> Columns { get; set; } = new List<AttributeDefinition>();
        public bool Clustered { get; set; }
        public Pointer RootPointer { get; set; }
    }
}

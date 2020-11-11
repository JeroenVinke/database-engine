using DatabaseEngine.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseEngine
{
    [DebuggerDisplay("Id: {Id}, Name: {Name}")]
    public class Relation : List<AttributeDefinition>
    {
        [FromColumn("Id")]
        public int Id { get; internal set; }
        [FromColumn("Name")]
        public string Name { get; set; }

        public void SyncRelation()
        {
            foreach(AttributeDefinition attributeDefinition in this)
            {
                attributeDefinition.Relation = this;
            }
        }
    }
}

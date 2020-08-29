using DatabaseEngine.Models;
using System.Collections.Generic;

namespace DatabaseEngine
{
    public class Relation : List<AttributeDefinition>
    {
        [FromColumn("Id")]
        public int Id { get; internal set; }
        [FromColumn("Name")]
        public string Name { get; set; }
    }
}

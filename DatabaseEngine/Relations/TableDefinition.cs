using DatabaseEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class TableDefinition : Relation
    {
        public List<Index> Indexes { get; set; } = new List<Index>();
        [FromColumn("RootBlockId")]
        public int RootBlockId { get; set; }

        public TableDefinition()
        {
            //Add(new AttributeDefinition
            //{
            //    Name = "RowId",
            //    Type = ValueType.Integer
            //});
        }

        public Index GetClusteredIndex()
        {
            return Indexes.First(x => x.IsClustered);
        }

        public void AddIndex(Index index)
        {
            Indexes.Add(index);
        }

        public bool HasClusteredIndex()
        {
            return Indexes.Any(x => x.IsClustered);
        }

        public IEnumerable<Index> GetIndexes()
        {
            return Indexes;
        }

        internal IEnumerable<Index> NonClusteredIndexes()
        {
            return Indexes.Where(x => !x.IsClustered);
        }

        internal AttributeDefinition GetAttributeByName(string v)
        {
            return this.First(x => x.Name.ToLower() == v.ToLower());
        }
    }
}

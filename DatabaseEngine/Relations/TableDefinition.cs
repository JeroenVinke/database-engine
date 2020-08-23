using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class TableDefinition : Relation
    {
        public List<Index> Indexes { get; set; } = new List<Index>();

        public void AddClusteredIndex(List<AttributeDefinition> columns)
        {
            if (HasClusteredIndex())
            {
                throw new Exception("Only one clustered index allowed");
            }

            Indexes.Add(new Index
            {
                Clustered = true,
                Columns = columns
            });
        }

        public Index GetClusteredIndex()
        {
            return Indexes.First(x => x.Clustered);
        }

        public void AddNonClusteredIndex(List<AttributeDefinition> columns)
        {
            Indexes.Add(new Index
            {
                Clustered = false,
                Columns = columns
            });
        }

        public bool HasClusteredIndex()
        {
            return Indexes.Any(x => x.Clustered);
        }

        public IEnumerable<Index> GetIndexes()
        {
            return Indexes;
        }
    }
}

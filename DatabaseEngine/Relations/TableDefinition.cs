using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class TableDefinition : Relation
    {
        private List<Index> Indexes { get; set; } = new List<Index>();
        public int MaxRecordSize
        {
            get
            {
                return this.Sum(x => x.Size);
            }
        }

        internal void AddClusteredIndex(List<AttributeDefinition> columns)
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

        internal Index GetClusteredIndex()
        {
            return Indexes.First(x => x.Clustered);
        }

        internal void AddNonClusteredIndex(List<AttributeDefinition> columns)
        {
            Indexes.Add(new Index
            {
                Clustered = false,
                Columns = columns
            });
        }

        internal bool HasClusteredIndex()
        {
            return Indexes.Any(x => x.Clustered);
        }
    }
}

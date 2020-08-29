using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class TableDefinition : Relation
    {
        public List<Index> Indexes { get; set; } = new List<Index>();

        public TableDefinition()
        {
            //Add(new AttributeDefinition
            //{
            //    Name = "RowId",
            //    Type = ValueType.Integer
            //});
        }

        public void AddClusteredIndex(List<AttributeDefinition> columns, int rootPointer)
        {
            if (HasClusteredIndex())
            {
                throw new Exception("Only one clustered index allowed");
            }

            Indexes.Add(new Index
            {
                Clustered = true,
                Columns = columns,
                RootPointer = new Pointer(rootPointer)
            });
        }

        public Index GetClusteredIndex()
        {
            return Indexes.First(x => x.Clustered);
        }

        public void AddNonClusteredIndex(List<AttributeDefinition> columns, int rootPointer)
        {
            Indexes.Add(new Index
            {
                Clustered = false,
                Columns = columns,
                RootPointer = new Pointer(rootPointer)
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

        internal IEnumerable<Index> NonClusteredIndexes()
        {
            return Indexes.Where(x => !x.Clustered);
        }

        internal AttributeDefinition GetAttributeByName(string v)
        {
            return this.First(x => x.Name.ToLower() == v.ToLower());
        }
    }
}

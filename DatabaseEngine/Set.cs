using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class Set
    {
        public Relation Relation { get; set; }
        public List<CustomTuple> Records { get; set; }

        public Set(Relation productRelation)
        {
            Relation = productRelation;
            Records = new List<CustomTuple>();
        }

        public void Add(CustomTuple tuple)
        {
            Records.Add(tuple);
        }

        public Set Union(Set otherSet)
        {
            Set union = new Set(Relation);

            foreach (CustomTuple record in All())
            {
                union.Add(record);
            }

            foreach (CustomTuple record in otherSet.All())
            {
                union.Add(record);
            }

            return union;
        }

        public Set Intersect(Set otherSet)
        {
            Set largest;
            Set smallest;

            if (Count() > otherSet.Count())
            {
                largest = this;
                smallest = otherSet;
            }
            else
            {
                largest = otherSet;
                smallest = this;
            }

            Set intersection = new Set(Relation);

            foreach(CustomTuple record in largest.All())
            {
                foreach(CustomTuple otherRecord in smallest.All())
                {
                    if (record.IsEqualTo(otherRecord))
                    {
                        intersection.Add(record);
                    }
                }
            }

            return intersection;
        }

        public CustomTuple Add(object[] entries)
        {
            CustomTuple tuple = new CustomTuple(Relation);

            foreach (object entry in entries)
            {
                tuple.Add(entry);
            }

            Add(tuple);

            return tuple;
        }

        public IEnumerable<CustomTuple> All()
        {
            return Records;
        }

        public int Count()
        {
            return Records.Count;
        }

        public Set Projection(List<string> attributeNames)
        {
            Relation projectionRelation = new Relation();

            foreach(string attributeName in attributeNames)
            {
                projectionRelation.Add(Relation.First(x => string.Equals(x.Name, attributeName, StringComparison.Ordinal)));
            }

            HashSet<string> attributeNamesHashSet = new HashSet<string>(attributeNames);

            // clone of entries weghalen?
            Set projectedSet = new Set(projectionRelation);

            foreach(CustomTuple record in Records)
            {
                CustomTuple projectedRecord = new CustomTuple(projectionRelation);
                
                foreach(CustomObject entry in record.Entries)
                {
                    if (attributeNamesHashSet.Contains(entry.AttributeDefinition.Name))
                    {
                        projectedRecord.Add(entry.Value);
                    }
                }

                projectedSet.Add(projectedRecord);
            }

            return projectedSet;
        }

        public CustomTuple First()
        {
            return Records[0];
        }
    }
}

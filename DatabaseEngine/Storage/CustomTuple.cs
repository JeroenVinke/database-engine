using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class CustomTuple
    {
        public List<CustomObject> Entries { get; set; } = new List<CustomObject>();
        private Relation Relation { get; set; }

        public CustomTuple(Relation relation)
        {
            Relation = relation;
        }

        internal bool IsEqualTo(CustomTuple otherRecord)
        {
            foreach (CustomObject obj in Entries)
            {
                bool found = false;
                foreach (CustomObject otherObj in otherRecord.Entries)
                {
                    if (obj.IsEqualTo(otherObj))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        internal CustomTuple WithEntries(object[] tuple)
        {
            foreach(object obj in tuple)
            {
                Add(obj);
            }

            return this;
        }

        internal bool Joins(CustomTuple rightTuple, AttributeDefinition leftJoinColumn, AttributeDefinition rightJoinColumn)
        {
            CustomObject left = GetEntryFor(leftJoinColumn.Name);
            CustomObject right = rightTuple.GetEntryFor(rightJoinColumn.Name);

            if (left.IsEqualTo(right))
            {
                return true;
            }

            return false;
        }

        public CustomObject GetEntryFor(string columnName)
        {
            return Entries.First(x => x.AttributeDefinition.Name == columnName);
        }

        public T GetValueFor<T>(string columnName)
        {
            int index = Entries.IndexOf(GetEntryFor(columnName));
            return (T)Entries[index].Value;
        }

        public CustomTuple Add(object value)
        {
            Entries.Add(new CustomObject() { Value = value, AttributeDefinition = Relation[Entries.Count] });
            return this;
        }

        public CustomTuple AddValueFor<TKeyType>(string v, TKeyType value) where TKeyType : IComparable
        {
            Entries.Add(new CustomObject() { Value = value, AttributeDefinition = Relation.First(x => x.Name == v) });
            return this;
        }

        internal CustomTuple FromRecord(Record record)
        {
            int i = 0;
            int ii = 0;
            foreach (AttributeDefinition attributeDefinition in Relation)
            {
                int nextOffsetBytes = record.Offsets.Count > ii + 1 ? record.Offsets[ii + 1].Bytes : record.Content.Length;
                byte[] entryBytes = record.Content.Skip(i).Take(nextOffsetBytes - record.Offsets[ii].Bytes).ToArray();

                Entries.Add(CustomObject.FromBytes(entryBytes, attributeDefinition));

                i += entryBytes.Length;
                ii++;
            }

            return this;
        }

        internal Record ToRecord()
        {
            Record record = new Record();

            List<byte> bytes = new List<byte>();
            int i = 0;
            foreach (CustomObject entry in Entries)
            {
                record.Offsets.Add(new Offset() { Bytes = i });
                byte[] entryBytes = entry.ToBytes();
                i += entryBytes.Length;
                bytes.AddRange(entryBytes);
            }

            record.Content = bytes.ToArray();

            return record;
        }
    }
}

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

        public void Add(object value)
        {
            Entries.Add(new CustomObject() { Value = value, AttributeDefinition = Relation[Entries.Count] });
        }

        internal CustomTuple FromRecord(Record record)
        {
            int i = 0;
            int ii = 0;
            foreach (AttributeDefinition attributeDefinition in ((TableDefinition)Relation))
            {
                if (attributeDefinition.IsFixedSize)
                {
                    byte[] entryBytes = record.Content.Skip(i).Take(attributeDefinition.Size).ToArray();

                    Entries.Add(CustomObject.FromBytes(entryBytes, attributeDefinition));

                    i += attributeDefinition.Size;
                }
                else
                {
                    Offset nextOffset = record.Offsets.Count > i + 1 ? record.Offsets[i + 1] : new Offset { Bytes = record.Content.Length - 1 };
                    byte[] entryBytes = record.Content.Skip(i).Take(nextOffset.Bytes - record.Offsets[ii].Bytes).ToArray();

                    Entries.Add(CustomObject.FromBytes(entryBytes, attributeDefinition));

                    i += entryBytes.Length;
                    ii++;
                }
            }

            return this;
        }

        internal Record ToRecord()
        {
            Record record = new Record
            {
                SchemaPointer = ((TableDefinition)Relation).Id
            };

            List<byte> bytes = new List<byte>();
            int i = 0;
            foreach (CustomObject entry in Entries)
            {
                if (!entry.AttributeDefinition.IsFixedSize)
                {
                    record.Offsets.Add(new Offset() { Bytes = i });
                }
                byte[] entryBytes = entry.ToBytes();
                i += entryBytes.Length;
                bytes.AddRange(entryBytes);
            }

            record.Content = bytes.ToArray();

            return record;
        }
    }
}

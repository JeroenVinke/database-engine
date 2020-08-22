using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class Record
    {
        public List<Offset> Offsets { get; set; } = new List<Offset>();
        public int SchemaPointer { get; set; }
        public const int HeaderSize = 6;
        public byte[] Content { get; set; }
        public const int SizeBytes = 2;
        public Offset OffsetInBlock { get; internal set; }

        public Record(byte[] bytes)
        {
            int schemaPointer = BitConverter.ToUInt16(new byte[] { bytes[0], bytes[1], bytes[2], bytes[3] });
            ushort sizeInBytes = BitConverter.ToUInt16(new byte[] { bytes[4], bytes[5] });

            SchemaPointer = schemaPointer;

            Relation tableDefinition = Program.Relations.First(x => x.Id == schemaPointer);
            for (int i = 0; i < tableDefinition.Where(x => !x.IsFixedSize).Count(); i++)
            {
                Offset offset = new Offset
                {
                    Bytes = BitConverter.ToUInt16(new byte[] { bytes[HeaderSize + i], bytes[HeaderSize + i + 1] })
                };
                Offsets.Add(offset);
            }

            Content = bytes.Skip(HeaderSize + (tableDefinition.Count(x => !x.IsFixedSize) * 2) + SizeBytes).ToArray();
        }

        public Record()
        {

        }

        public int Length
        {
            get
            {
                return Content.Length + HeaderSize + (Offsets.Count * 2) + SizeBytes;
            }
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(SchemaPointer));
            bytes.AddRange(BitConverter.GetBytes(Length));
            foreach (Offset offset in Offsets)
            {
                bytes.AddRange(offset.GetOffsetInBytes());
            }
            bytes.AddRange(Content);

            return bytes.ToArray();
        }
    }
}

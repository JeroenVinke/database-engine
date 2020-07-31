using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class DataRecord: Record
    {
        public List<Offset> Offsets { get; set; } = new List<Offset>();
        public int SchemaPointer { get; set; }
        public const int HeaderSize = 4;
        public const int SizeBytes = 2;

        public override int Length
        {
            get
            {
                return Content.Length + HeaderSize + (Offsets.Count * 2) + SizeBytes;
            }
        }

        public static DataRecord FromBytes(byte[] bytes)
        {
            ushort schemaPointer = BitConverter.ToUInt16(new byte[] { bytes[0], bytes[1] });
            ushort sizeInBytes = BitConverter.ToUInt16(new byte[] { bytes[2], bytes[3] });

            DataRecord record = new DataRecord();
            record.SchemaPointer = schemaPointer;

            TableDefinition tableDefinition = Program.Tables.First(x => x.Id == schemaPointer);
            for (int i = 0; i < tableDefinition.Where(x => !x.IsFixedSize).Count(); i++)
            {
                Offset offset = new Offset
                {
                    Bytes = BitConverter.ToUInt16(new byte[] { bytes[4 + i], bytes[4 + i + 1] })
                };
                record.Offsets.Add(offset);
            }

            record.Content = bytes.Skip(HeaderSize + (tableDefinition.Count(x => !x.IsFixedSize) * 2) + SizeBytes).ToArray();

            return record;
        }

        public override byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(((ushort)SchemaPointer)));
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

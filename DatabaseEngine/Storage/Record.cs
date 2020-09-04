using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class Record
    {
        public List<Offset> Offsets { get; set; } = new List<Offset>();
        public byte[] Content { get; set; }

        public Record(Relation relation, byte[] bytes)
        {
            int offsetBytes = 0;
            int offsetSize = new Offset().Size;
            for (int i = 0; i < relation.Count(); i++)
            {
                Offset offset = new Offset
                {
                    Bytes = BitConverter.ToUInt16(new byte[] { bytes[i * offsetSize], bytes[(i * offsetSize) + 1] })
                };
                Offsets.Add(offset);
                offsetBytes += offsetSize;
            }

            Content = bytes.Skip(offsetBytes).ToArray();
        }

        public Record()
        {

        }

        public int Length
        {
            get
            {
                return ToBytes().Length;
            }
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            foreach (Offset offset in Offsets)
            {
                bytes.AddRange(offset.GetOffsetInBytes());
            }
            bytes.AddRange(Content);

            return bytes.ToArray();
        }
    }
}

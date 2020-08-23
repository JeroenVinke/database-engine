using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class Record
    {
        public List<Offset> Offsets { get; set; } = new List<Offset>();
        public byte[] Content { get; set; }
        public Offset OffsetInBlock { get; internal set; }

        public Record(Relation relation, byte[] bytes)
        {
            for (int i = 0; i < relation.Count(); i++)
            {
                Offset offset = new Offset
                {
                    Bytes = BitConverter.ToUInt16(new byte[] { bytes[i*4], bytes[(i*4) + 1] })
                };
                Offsets.Add(offset);
            }

            Content = bytes.Skip(relation.Count() * 4).ToArray();
        }

        public Record()
        {

        }

        public int Length
        {
            get
            {
                return Content.Length + (Offsets.Count * 4);
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

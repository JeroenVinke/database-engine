using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class RecordOffset : Offset
    {
        public int Id { get; set; }

        public RecordOffset(int bytes, int id) : base(bytes)
        {
            Id = id;
        }

        public RecordOffset(byte[] bytes)
        {
            Bytes = BitConverter.ToInt32(bytes.Take(4).ToArray(), 0);
            Id = BitConverter.ToInt32(bytes.Skip(4).Take(4).ToArray(), 0);
        }

        public override int Size => 8;

        public override byte[] GetOffsetInBytes()
        {
            List<byte> result = BitConverter.GetBytes(Bytes).ToList();
            result.AddRange(BitConverter.GetBytes(Id));
            return result.ToArray();
        }
    }
}

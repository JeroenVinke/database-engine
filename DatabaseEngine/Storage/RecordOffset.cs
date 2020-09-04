using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class RecordOffset : Offset
    {
        public ushort Id { get; set; }

        public RecordOffset(ushort bytes, ushort id) : base(bytes)
        {
            Id = id;
        }

        public RecordOffset(byte[] bytes)
        {
            Bytes = BitConverter.ToUInt16(bytes.Take(2).ToArray(), 0);
            Id = BitConverter.ToUInt16(bytes.Skip(2).Take(2).ToArray(), 0);
        }

        public override int Size => 4;

        public override byte[] GetOffsetInBytes()
        {
            List<byte> result = BitConverter.GetBytes(Bytes).ToList();
            result.AddRange(BitConverter.GetBytes(Id));
            return result.ToArray();
        }
    }
}

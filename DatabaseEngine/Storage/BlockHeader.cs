using System;
using System.Collections.Generic;

namespace DatabaseEngine
{
    public class BlockHeader
    {
        public List<Offset> Offsets { get; set; } = new List<Offset>();
        public BlockBuffer Buffer { get; set; }
        public bool Empty { get; set; }

        public int RelationId { get; set; }

        public BlockHeader(BlockBuffer buffer)
        {
            Buffer = buffer;

            Empty = BitConverter.ToInt32(buffer.ReadBytes(4)) == 0;
            RelationId = BitConverter.ToInt32(buffer.ReadBytes(4));
            int offsetCount = BitConverter.ToInt32(buffer.ReadBytes(4), 0);
            ReadOffsets(buffer, offsetCount);
        }

        public BlockHeader()
        {
        }

        private void ReadOffsets(BlockBuffer buffer, int offsetCount)
        {
            for (int i = 0; i < offsetCount; i++)
            {
                int offsetShort = BitConverter.ToInt32(buffer.ReadBytes(4), 0);
                Offset offset = new Offset()
                {
                    Bytes = offsetShort
                };
                Offsets.Add(offset);
            }
        }

        public IEnumerable<byte> ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Empty ? 0 : 1));
            bytes.AddRange(BitConverter.GetBytes(RelationId));
            bytes.AddRange(BitConverter.GetBytes(Offsets.Count));

            foreach (Offset offset in Offsets)
            {
                byte[] offsetBytes = offset.GetOffsetInBytes();
                bytes.AddRange(offsetBytes);
            }

            return bytes.ToArray();
        }
    }
}
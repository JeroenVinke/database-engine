using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class BlockHeader
    {
        public List<RecordOffset> Offsets { get; set; } = new List<RecordOffset>();
        public BlockBuffer Buffer { get; set; }
        public bool Empty { get; set; }

        public int RelationId { get; set; }
        public Pointer NextBlockId { get; set; }

        public BlockHeader(BlockBuffer buffer)
        {
            Buffer = buffer;

            Empty = BitConverter.ToInt32(buffer.ReadBytes(4)) == 0;
            RelationId = BitConverter.ToInt32(buffer.ReadBytes(4));
            NextBlockId = new Pointer(BitConverter.ToInt32(buffer.ReadBytes(4)));
            int offsetCount = BitConverter.ToInt32(buffer.ReadBytes(4), 0);
            ReadOffsets(buffer, offsetCount);
        }

        public BlockHeader()
        {
        }

        public int Size => ToBytes().Count();

        private void ReadOffsets(BlockBuffer buffer, int offsetCount)
        {
            for (int i = 0; i < offsetCount; i++)
            {
                RecordOffset offset = new RecordOffset(buffer.ReadBytes(8));
                Offsets.Add(offset);
            }
        }

        public IEnumerable<byte> ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Empty ? 0 : 1));
            bytes.AddRange(BitConverter.GetBytes(RelationId));
            bytes.AddRange(BitConverter.GetBytes(NextBlockId != null ? NextBlockId.Short : 0));
            bytes.AddRange(BitConverter.GetBytes(Offsets.Count));

            foreach (RecordOffset offset in Offsets)
            {
                byte[] offsetBytes = offset.GetOffsetInBytes();
                bytes.AddRange(offsetBytes);
            }

            return bytes.ToArray();
        }
    }
}
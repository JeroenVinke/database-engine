using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class BlockHeader
    {
        public List<RecordOffset> Offsets { get; set; } = new List<RecordOffset>();
        public BlockBuffer Buffer { get; set; }
        public bool IsFilled { get; set; }

        public int RelationId { get; set; }
        public Pointer NextBlockId { get; set; }

        public BlockHeader(BlockBuffer buffer)
        {
            Buffer = buffer;

            IsFilled = buffer.ReadByte() == 1;
            RelationId = BitConverter.ToInt32(buffer.ReadBytes(4));
            NextBlockId = new Pointer(BitConverter.ToUInt16(buffer.ReadBytes(4)));
            ushort offsetCount = BitConverter.ToUInt16(buffer.ReadBytes(2), 0);
            ReadOffsets(buffer, offsetCount);
        }

        public BlockHeader()
        {
        }

        public int Size => ToBytes().Count();

        private void ReadOffsets(BlockBuffer buffer, ushort offsetCount)
        {
            int recordOffsetSize = new RecordOffset(0, 0).Size;

            for (int i = 0; i < offsetCount; i++)
            {
                RecordOffset offset = new RecordOffset(buffer.ReadBytes(recordOffsetSize));
                Offsets.Add(offset);
            }
        }

        public IEnumerable<byte> ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(IsFilled ? (byte)1 : (byte)0);
            bytes.AddRange(BitConverter.GetBytes(RelationId));
            bytes.AddRange(BitConverter.GetBytes(NextBlockId != null ? NextBlockId.Short : 0));
            bytes.AddRange(BitConverter.GetBytes((ushort)Offsets.Count));

            foreach (RecordOffset offset in Offsets)
            {
                byte[] offsetBytes = offset.GetOffsetInBytes();
                bytes.AddRange(offsetBytes);
            }

            return bytes.ToArray();
        }
    }
}
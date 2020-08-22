using System;
using System.Collections.Generic;

namespace DatabaseEngine
{
    public abstract class BlockHeader
    {
        public List<Offset> Offsets { get; set; } = new List<Offset>();
        public abstract BlockType Type { get; }
        public BlockBuffer Buffer { get; set; }

        public BlockHeader(BlockBuffer buffer)
        {
            Buffer = buffer;

            buffer.ReadByte(); // type
            ReadCustomHeaderFromBuffer(buffer);
            ushort offsetCount = ReadOffsetCount(buffer);
            ReadOffsets(buffer, offsetCount);
        }

        public BlockHeader()
        {
        }

        private void ReadOffsets(BlockBuffer buffer, ushort offsetCount)
        {
            for (int i = 0; i < offsetCount; i++)
            {
                ushort offsetShort = BitConverter.ToUInt16(buffer.ReadBytes(2), 0);
                Offset offset = new Offset()
                {
                    Bytes = offsetShort
                };
                Offsets.Add(offset);
            }
        }

        private static ushort ReadOffsetCount(BlockBuffer buffer)
        {
            return BitConverter.ToUInt16(buffer.ReadBytes(2), 0);
        }

        public IEnumerable<byte> ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Type);
            bytes.AddRange(GetCustomHeaderBytes());
            bytes.AddRange(BitConverter.GetBytes((ushort)Offsets.Count));

            foreach (Offset offset in Offsets)
            {
                byte[] offsetBytes = offset.GetOffsetInBytes();
                bytes.AddRange(offsetBytes);
            }

            return bytes.ToArray();
        }

        protected virtual byte[] GetCustomHeaderBytes()
        {
            return new byte[0];
        }

        protected virtual void ReadCustomHeaderFromBuffer(BlockBuffer buffer)
        {
        }
    }
}
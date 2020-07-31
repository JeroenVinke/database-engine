using System;
using System.Collections.Generic;

namespace DatabaseEngine
{
    public abstract class BlockHeader
    {
        public List<Offset> Offsets = new List<Offset>();
        public BlockType Type
        {
            get
            {
                return this is DataBlockHeader ? BlockType.Data : BlockType.Index;
            }
        }
        public BlockBuffer Buffer { get; set; }

        public BlockHeader(BlockBuffer buffer)
        {
            Buffer = buffer;
        }

        public BlockHeader()
        {
        }

        public static BlockHeader CreateHeader(BlockBuffer buffer)
        {
            BlockType type = (BlockType)(int)buffer.ReadByte();

            if (type == BlockType.Index)
            {
                return IndexBlockHeader.CreateIndexHeader(buffer);
            }
            else if(type == BlockType.Data)
            {
                return DataBlockHeader.CreateDataHeader(buffer);
            }

            return null;
        }

        internal void ReadOffsets(BlockBuffer buffer, ushort offsetCount)
        {
            for (int i = 2; i < (offsetCount * 2) + 2; i++)
            {
                ushort offsetShort = BitConverter.ToUInt16(buffer.ReadBytes(2), 0);
                Offset offset = new Offset()
                {
                    Bytes = offsetShort
                };
                Offsets.Add(offset);
            }
        }

        public static ushort ReadOffsetCount(BlockBuffer buffer)
        {
            return BitConverter.ToUInt16(buffer.ReadBytes(2), 0);
        }

        internal IEnumerable<byte> ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Type);

            int i = 0;
            foreach (Offset offset in Offsets)
            {
                byte[] offsetBytes = offset.GetOffsetInBytes();
                for (int ii = 0; ii < offsetBytes.Length; ii++)
                {
                    bytes[i + ii] = offsetBytes[ii];
                }
                i += 2;
            }

            return bytes.ToArray();
        }
    }
}
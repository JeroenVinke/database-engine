using System;

namespace DatabaseEngine
{
    public class DataBlockHeader : BlockHeader
    {
        public DataBlockHeader(BlockBuffer buffer)
            : base(buffer)
        {
        }

        public DataBlockHeader()
        {
        }

        public static BlockHeader CreateDataHeader(BlockBuffer buffer)
        {
            BlockHeader header = new DataBlockHeader(buffer);

            ushort offsetCount = ReadOffsetCount(buffer);
            header.ReadOffsets(buffer, offsetCount);

            return header;
        }
    }
}
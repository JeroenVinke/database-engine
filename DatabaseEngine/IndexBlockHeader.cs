namespace DatabaseEngine
{
    public class IndexBlockHeader : BlockHeader
    {
        public IndexBlockHeader(BlockBuffer buffer)
            : base(buffer)
        {
        }

        public IndexBlockHeader()
        {
        }

        public static BlockHeader CreateIndexHeader(BlockBuffer buffer)
        {
            BlockHeader header = new DataBlockHeader(buffer);

            ushort offsetCount = ReadOffsetCount(buffer);
            header.ReadOffsets(buffer, offsetCount);

            return header;
        }
    }
}
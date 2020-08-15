namespace DatabaseEngine
{
    public class IndexBlockHeader : BlockHeader
    {
        public override BlockType Type => BlockType.Index;

        public IndexBlockHeader(BlockBuffer buffer)
            : base(buffer)
        {
        }

        public IndexBlockHeader()
        {
        }

        public static BlockHeader CreateIndexHeader(BlockBuffer buffer)
        {
            BlockHeader header = new IndexBlockHeader(buffer);

            return header;
        }
    }
}
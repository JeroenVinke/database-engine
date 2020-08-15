namespace DatabaseEngine
{
    public class EmptyBlockHeader : BlockHeader
    {
        public override BlockType Type => BlockType.Free;

        public EmptyBlockHeader(BlockBuffer buffer)
            : base(buffer)
        {
        }

        public EmptyBlockHeader()
        {
        }
    }
}
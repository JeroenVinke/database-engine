namespace DatabaseEngine
{
    public class IndexBlock : Block
    {
        public IndexBlock() : base()
        {
            Header = new IndexBlockHeader();
        }
    }
}

namespace DatabaseEngine
{
    public class EmptyBlock : Block
    {
        public EmptyBlock() : base()
        {
            Header = new EmptyBlockHeader();
        }
    }
}

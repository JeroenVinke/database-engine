namespace DatabaseEngine.LogicalPlan
{
    public abstract class ReadLogicalElement : LogicalElement
    {
        public new ReadLogicalElement LeftChild => (ReadLogicalElement)base.LeftChild;
        public new ReadLogicalElement RightChild => (ReadLogicalElement)base.RightChild;

        public ReadLogicalElement() : base()
        {
        }

        public ReadLogicalElement(ReadLogicalElement child) : base(child)
        {
        }

        public ReadLogicalElement(ReadLogicalElement leftChild, ReadLogicalElement rightChild) : base(leftChild, rightChild)
        {
        }

        public abstract int T();
        public abstract double V(AttributeDefinition column);
    }
}

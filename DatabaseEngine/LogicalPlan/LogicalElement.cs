namespace DatabaseEngine.LogicalPlan
{
    public class LogicalElement
    {
        public LogicalElement LeftChild { get; set; }
        public LogicalElement RightChild { get; set; }

        public LogicalElement()
        {

        }

        public LogicalElement(LogicalElement child)
        {
            LeftChild = child;
        }

        public LogicalElement(LogicalElement leftChild, LogicalElement rightChild)
        {
            LeftChild = leftChild;
            RightChild = rightChild;
        }

        public override string ToString()
        {
            return base.ToString() + System.Environment.NewLine + "-- " + LeftChild.ToString() + " | " + RightChild.ToString();
        }
    }
}

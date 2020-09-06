namespace DatabaseEngine
{
    public class OrCondition : Condition
    {
        public Condition Left { get; set; }
        public Condition Right { get; set; }

        public override string ToString()
        {
            return "(" + Left.ToString() + ") OR (" + Right.ToString() + ")";
        }
    }
}

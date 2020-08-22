namespace DatabaseEngine
{
    public class OrCondition : Condition
    {
        public Condition Left { get; set; }
        public Condition Right { get; set; }
    }
}

using Compiler.Common;

namespace DatabaseEngine
{
    public class AndCondition : Condition
    {
        public Condition Left { get; set; }
        public Condition Right { get; set; }

        public override Condition Clone()
        {
            return new AndCondition()
            {
                Left = Left?.Clone(),
                Right = Right?.Clone()
            };
        }

        public override string ToString()
        {
            return "(" + Left.ToString() + ") AND (" + Right.ToString() + ")";
        }
    }
}

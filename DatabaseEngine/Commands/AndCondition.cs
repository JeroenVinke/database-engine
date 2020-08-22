using Compiler.Common;

namespace DatabaseEngine
{
    public class AndCondition : Condition
    {
        public Condition Left { get; set; }
        public Condition Right { get; set; }
    }
}

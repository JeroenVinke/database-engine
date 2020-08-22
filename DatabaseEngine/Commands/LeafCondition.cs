using Compiler.Common;

namespace DatabaseEngine
{
    public class LeafCondition : Condition
    {
        public AttributeDefinition Column { get; set; }
        public RelOp Operation { get; set; }
        public object Value { get; set; }
    }
}

using Compiler.Common;
using Compiler.Parser.SyntaxTreeNodes;

namespace DatabaseEngine
{
    public class LeafCondition : Condition
    {
        public AttributeDefinition Column { get; set; }
        public RelOp Operation { get; set; }
        public object Value { get; set; }
        public bool AlwaysTrue { get; set; }

        public override Condition Clone()
        {
            return new LeafCondition()
            {
                Column = Column,
                Operation = Operation,
                Value = Value,
                AlwaysTrue = AlwaysTrue
            };
        }

        public override string ToString()
        {
            return Column.Name + " " + new RelOpASTNode(null, Operation, null).RelOpAsString + " " + Value.ToString() + " " + (AlwaysTrue ? " ALWAYS TRUE!" : "");
        }
    }
}

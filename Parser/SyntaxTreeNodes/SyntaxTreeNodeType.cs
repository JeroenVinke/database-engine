namespace Compiler.Parser
{
    public enum SyntaxTreeNodeType
    {
        Leaf,
        Plus,
        Min,
        Equals,
        And,
        Or,
        BooleanExpression,
        Boolean,
        NumericExpression,
        Identifier,
        Number,
        RelOp,
        Select,
        From,
        String,
        Join
    }
}
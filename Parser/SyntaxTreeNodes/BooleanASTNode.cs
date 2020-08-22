namespace Compiler.Parser.SyntaxTreeNodes
{
    public class BooleanASTNode : BooleanExpressionASTNode
    {
        public bool Value { get; set; }

        public BooleanASTNode(bool value) : base(SyntaxTreeNodeType.Boolean)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value ? "true" : "false";
        }
    }
}

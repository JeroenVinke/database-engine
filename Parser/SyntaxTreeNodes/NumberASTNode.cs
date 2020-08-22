namespace Compiler.Parser.SyntaxTreeNodes
{
    public class NumberASTNode : NumericExpressionASTNode
    {
        public int Value { get; set; }

        public NumberASTNode() : base(SyntaxTreeNodeType.Number)
        {
        }

        public override string ToString()
        {
            return "" + Value;
        }
    }
}

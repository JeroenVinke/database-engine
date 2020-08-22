namespace Compiler.Parser.SyntaxTreeNodes
{
    public class StringASTNode : FactorASTNode
    {
        public string Value { get; set; }

        public StringASTNode(string value) : base(SyntaxTreeNodeType.String)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}

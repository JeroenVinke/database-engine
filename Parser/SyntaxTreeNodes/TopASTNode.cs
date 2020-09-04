namespace Compiler.Parser.SyntaxTreeNodes
{
    public class TopASTNode : SyntaxTreeNode
    {
        public int Amount { get; set; }

        public TopASTNode() : base(SyntaxTreeNodeType.Number)
        {
        }

        public override string ToString()
        {
            return "" + Amount;
        }
    }
}

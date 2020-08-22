using System.Collections.Generic;

namespace Compiler.Parser.SyntaxTreeNodes
{
    public class SelectASTNode : SyntaxTreeNode
    {
        public FromASTNode From { get; set; }
        public BooleanExpressionASTNode Condition { get; set; }

        public SelectASTNode() : base(SyntaxTreeNodeType.Select)
        {
        }

        protected override List<SyntaxTreeNode> GetChildren()
        {
            return new List<SyntaxTreeNode> { From, Condition };
        }

        public override string ToString()
        {
            return "SELECT";
        }
    }
}

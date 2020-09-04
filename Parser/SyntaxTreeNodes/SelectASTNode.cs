using System.Collections.Generic;

namespace Compiler.Parser.SyntaxTreeNodes
{
    public class SelectASTNode : SyntaxTreeNode
    {
        public List<FactorASTNode> SelectColumns { get; set; }
        public FromASTNode From { get; set; }
        public BooleanExpressionASTNode Condition { get; set; }
        public JoinASTNode Join { get; set; }
        public TopASTNode Top { get; set; }

        public SelectASTNode() : base(SyntaxTreeNodeType.Select)
        {
        }

        protected override List<SyntaxTreeNode> GetChildren()
        {
            var nodes = new List<SyntaxTreeNode> { From, Condition, Join, Top };
            nodes.AddRange(SelectColumns);
            return nodes;
        }

        public override string ToString()
        {
            return "SELECT";
        }
    }
}

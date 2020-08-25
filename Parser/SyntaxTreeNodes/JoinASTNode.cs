using System.Collections.Generic;

namespace Compiler.Parser.SyntaxTreeNodes
{
    public class JoinASTNode : SyntaxTreeNode
    {
        public IdentifierASTNode TargetTable { get; set; }
        public IdentifierASTNode LeftColumn { get; set; }
        public IdentifierASTNode RightColumn { get; set; }

        public JoinASTNode() : base(SyntaxTreeNodeType.Join)
        {
        }

        protected override List<SyntaxTreeNode> GetChildren()
        {
            return new List<SyntaxTreeNode> { TargetTable, LeftColumn, RightColumn };
        }

        public override string ToString()
        {
            return "JOIN";
        }
    }
}

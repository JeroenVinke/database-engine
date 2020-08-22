using System.Collections.Generic;

namespace Compiler.Parser.SyntaxTreeNodes
{
    public class FromASTNode : SyntaxTreeNode
    {
        public IdentifierASTNode Identifier { get; set; }

        public FromASTNode() : base(SyntaxTreeNodeType.From)
        {
        }

        protected override List<SyntaxTreeNode> GetChildren()
        {
            return new List<SyntaxTreeNode> { Identifier };
        }

        public override string ToString()
        {
            return "FROM";
        }
    }
}

using System.Collections.Generic;

namespace Compiler.Parser.SyntaxTreeNodes
{
    public class InsertASTNode : SyntaxTreeNode
    {
        public IdentifierASTNode Into { get; set; }
        public List<FactorASTNode> Arguments { get; set; }

        public InsertASTNode() : base(SyntaxTreeNodeType.Insert)
        {
        }

        protected override List<SyntaxTreeNode> GetChildren()
        {
            var nodes = new List<SyntaxTreeNode> { Into };
            nodes.AddRange(Arguments);
            return nodes;
        }

        public override string ToString()
        {
            return "INSERT";
        }
    }
}

using System.Collections.Generic;

namespace Compiler.Parser.SyntaxTreeNodes
{
    public class AdditionASTNode : NumericExpressionASTNode
    {
        public FactorASTNode Left { get; private set; }
        public FactorASTNode Right { get; private set; }

        public AdditionASTNode(FactorASTNode left, FactorASTNode right) : base(SyntaxTreeNodeType.Plus)
        {
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return "+";
        }

        protected override List<SyntaxTreeNode> GetChildren()
        {
            return new List<SyntaxTreeNode> { Left, Right };
        }
    }
}

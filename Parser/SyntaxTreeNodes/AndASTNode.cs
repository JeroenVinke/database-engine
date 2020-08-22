using System.Collections.Generic;

namespace Compiler.Parser.SyntaxTreeNodes
{
    public class AndASTNode : BooleanExpressionASTNode
    {
        public BooleanExpressionASTNode Left { get; private set; }
        public BooleanExpressionASTNode Right { get; private set; }

        public AndASTNode(BooleanExpressionASTNode left, BooleanExpressionASTNode right) : base(SyntaxTreeNodeType.And)
        {
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return "&&";
        }

        protected override List<SyntaxTreeNode> GetChildren()
        {
            return new List<SyntaxTreeNode> { Left, Right };
        }
    }
}

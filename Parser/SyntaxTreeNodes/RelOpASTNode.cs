﻿
using Compiler.Common;
using System.Collections.Generic;

namespace Compiler.Parser.SyntaxTreeNodes
{
    public class RelOpASTNode : BooleanExpressionASTNode
    {
        public FactorASTNode Left { get; private set; }
        public FactorASTNode Right { get; private set; }
        public RelOp RelationOperator { get; private set; }
        public string RelOpAsString
        {
            get
            {
                if (RelationOperator == RelOp.Equals)
                {
                    return "==";
                }
                if (RelationOperator == RelOp.NotEquals)
                {
                    return "!=";
                }
                if (RelationOperator == RelOp.GreaterOrEqualThan)
                {
                    return ">=";
                }
                if (RelationOperator == RelOp.GreaterThan)
                {
                    return ">";
                }
                if (RelationOperator == RelOp.LessOrEqualThan)
                {
                    return "<=";
                }
                if (RelationOperator == RelOp.In)
                {
                    return "IN";
                }
                if (RelationOperator == RelOp.LessThan)
                {
                    return "<";
                }

                return "";
            }
        }

        public string OppositeRelOpAsString
        {
            get
            {
                if (RelationOperator == RelOp.Equals)
                {
                    return "!=";
                }
                if (RelationOperator == RelOp.In)
                {
                    return "NOT IN";
                }
                if (RelationOperator == RelOp.NotEquals)
                {
                    return "==";
                }
                if (RelationOperator == RelOp.GreaterOrEqualThan)
                {
                    return "<";
                }
                if (RelationOperator == RelOp.GreaterThan)
                {
                    return "<=";
                }
                if (RelationOperator == RelOp.LessOrEqualThan)
                {
                    return ">";
                }
                if (RelationOperator == RelOp.LessThan)
                {
                    return ">=";
                }

                return "";
            }
        }

        public RelOpASTNode(FactorASTNode left, RelOp relationOperator, FactorASTNode right) : base(SyntaxTreeNodeType.RelOp)
        {
            Left = left;
            Right = right;
            RelationOperator = relationOperator;
        }

        public override string ToString()
        {
            return RelOpAsString;
        }

        protected override List<SyntaxTreeNode> GetChildren()
        {
            return new List<SyntaxTreeNode> { Left, Right };
        }
    }
}

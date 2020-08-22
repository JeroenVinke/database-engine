using Compiler.Parser.SyntaxTreeNodes;
using System.Collections.Generic;

namespace Compiler.Parser.Rules
{
    public class FactorRule
    {
        public static void Initialize(ref Grammar grammar)
        {
            grammar.Add(new Production(ParserConstants.Factor,
                new List<SubProduction>
                {
                    IdentifierRule(),
                    //BooleanRule(),
                    NumExpressionRule(),
                    StringRule()
                }
            ));
        }

        private static SubProduction IdentifierRule()
        {
            return new SubProduction
            (
                new List<ExpressionDefinition>
                {
                    new NonTerminalExpressionDefinition { Identifier = ParserConstants.Identifier },
                    new SemanticActionDefinition((ParsingNode node) =>
                    {
                        node.Attributes.Add(ParserConstants.SyntaxTreeNode, node.GetAttributeForKey<SyntaxTreeNode>(ParserConstants.Identifier, ParserConstants.SyntaxTreeNode));
                    })
                }
            );
        }

        //private static SubProduction BooleanRule()
        //{
        //    return new SubProduction
        //    (
        //        new List<ExpressionDefinition>
        //        {
        //            new NonTerminalExpressionDefinition { Identifier = ParserConstants.Boolean },
        //            new SemanticActionDefinition((ParsingNode node) =>
        //            {
        //                node.Attributes.Add(ParserConstants.SyntaxTreeNode, node.GetAttributeForKey<SyntaxTreeNode>(ParserConstants.BooleanExpression, ParserConstants.SyntaxTreeNode));
        //            })
        //        }
        //    );
        //}

        private static SubProduction StringRule()
        {
            return new SubProduction
            (
                new List<ExpressionDefinition>
                {
                    new NonTerminalExpressionDefinition { Identifier = ParserConstants.StringExpression },
                    new SemanticActionDefinition((ParsingNode node) =>
                    {
                        node.Attributes.Add(ParserConstants.SyntaxTreeNode, node.GetAttributeForKey<SyntaxTreeNode>(ParserConstants.StringExpression, ParserConstants.SyntaxTreeNode));
                    })
                }
            );
        }

        private static SubProduction NumExpressionRule()
        {
            return new SubProduction
            (
                new List<ExpressionDefinition>
                {
                    new NonTerminalExpressionDefinition { Identifier = ParserConstants.NumericExpression },
                    new SemanticActionDefinition((ParsingNode node) =>
                    {
                        node.Attributes.Add(ParserConstants.SyntaxTreeNode, node.GetAttributeForKey<NumericExpressionASTNode>(ParserConstants.NumericExpression, ParserConstants.SyntaxTreeNode));
                    })
                }
            );
        }
    }
}
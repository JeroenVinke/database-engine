using Compiler.Common;
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

            grammar.Add(new Production(ParserConstants.Factors,
                new List<SubProduction>()
                {
                    new SubProduction
                    (
                        new List<ExpressionDefinition>
                        {
                            new NonTerminalExpressionDefinition { Identifier = ParserConstants.Factors },
                            new TerminalExpressionDefinition { TokenType = TokenType.Comma },
                            new NonTerminalExpressionDefinition { Identifier = ParserConstants.Factor },
                            new SemanticActionDefinition((ParsingNode node) =>
                            {
                                List<FactorASTNode> result = new List<FactorASTNode>();

                                List<FactorASTNode> factors = node.GetAttributeForKey<List<FactorASTNode>>(ParserConstants.Factors, ParserConstants.Factors);
                                FactorASTNode factor = node.GetAttributeForKey<FactorASTNode>(ParserConstants.Factor, ParserConstants.SyntaxTreeNode);

                                result.AddRange(factors);
                                result.Add(factor);

                                node.Attributes.Add(ParserConstants.Factors, result);
                            })
                        }
                    ),
                    new SubProduction
                    (
                        new List<ExpressionDefinition>
                        {
                            new NonTerminalExpressionDefinition { Identifier = ParserConstants.Factor },
                            new SemanticActionDefinition((ParsingNode node) =>
                            {
                                List<FactorASTNode> factors = new List<FactorASTNode>();
                                FactorASTNode factor = node.GetAttributeForKey<FactorASTNode>(ParserConstants.Factor, ParserConstants.SyntaxTreeNode);
                                factors.Add(factor);
                                node.Attributes.Add(ParserConstants.Factors, factors);
                            })
                        }
                    ),
                    new SubProduction
                    (
                        new List<ExpressionDefinition>
                        {
                            new TerminalExpressionDefinition { TokenType = TokenType.EmptyString },
                            new SemanticActionDefinition((ParsingNode node) =>
                            {
                                node.Attributes.Add(ParserConstants.Factors, new List<FactorASTNode>());
                            })
                        }
                    )
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
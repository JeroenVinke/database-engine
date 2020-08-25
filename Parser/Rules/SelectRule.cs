using Compiler.Common;
using Compiler.Parser.SyntaxTreeNodes;
using System;
using System.Collections.Generic;

namespace Compiler.Parser.Rules
{
    public class SelectRule
    {
        public static void Initialize(ref Grammar grammar)
        {
            grammar.Add(new Production(ParserConstants.Where, new List<SubProduction>
            {
                GetWhereRule(),
                new SubProduction
                (
                    new List<ExpressionDefinition>
                    {
                        new TerminalExpressionDefinition { TokenType = TokenType.EmptyString },
                        new SemanticActionDefinition((ParsingNode node) =>
                        {
                            node.Attributes.Add(ParserConstants.SyntaxTreeNode, null);
                        })
                    }
                )
            }));
            grammar.Add(new Production(ParserConstants.Join, new List<SubProduction>
            {
                GetJoinRule(),
                new SubProduction
                (
                    new List<ExpressionDefinition>
                    {
                        new TerminalExpressionDefinition { TokenType = TokenType.EmptyString },
                        new SemanticActionDefinition((ParsingNode node) =>
                        {
                            node.Attributes.Add(ParserConstants.SyntaxTreeNode, null);
                        })
                    }
                )
            }));
            grammar.Add(new Production(ParserConstants.Select, new List<SubProduction>
            {
                 GetSelectRule()
            }));
        }

        private static SubProduction GetJoinRule()
        {
            return new SubProduction
            (
                new List<ExpressionDefinition>
                {
                    new TerminalExpressionDefinition { TokenType = TokenType.Join },
                    new NonTerminalExpressionDefinition { Key = "Identifier1", Identifier = ParserConstants.Identifier },
                    new TerminalExpressionDefinition { TokenType = TokenType.On },
                    new NonTerminalExpressionDefinition { Key = "Identifier2", Identifier = ParserConstants.Identifier },
                    new TerminalExpressionDefinition { TokenType = TokenType.RelOp },
                    new NonTerminalExpressionDefinition { Key = "Identifier3", Identifier = ParserConstants.Identifier },
                    new SemanticActionDefinition((ParsingNode node) =>
                    {
                        JoinASTNode astNode = new JoinASTNode();
                        astNode.TargetTable = node.GetAttributeForKey<IdentifierASTNode>("Identifier1", ParserConstants.SyntaxTreeNode);
                        astNode.LeftColumn = node.GetAttributeForKey<IdentifierASTNode>("Identifier2", ParserConstants.SyntaxTreeNode);
                        astNode.RightColumn = node.GetAttributeForKey<IdentifierASTNode>("Identifier3", ParserConstants.SyntaxTreeNode);

                        node.Attributes.Add(ParserConstants.SyntaxTreeNode, astNode);
                    })
                }
            );
        }

        private static SubProduction GetSelectRule()
        {
            return new SubProduction
            (
                new List<ExpressionDefinition>
                {
                    new TerminalExpressionDefinition { TokenType = TokenType.Select },
                    new TerminalExpressionDefinition { TokenType = TokenType.Multiplication },
                    new TerminalExpressionDefinition { TokenType = TokenType.From },
                    new NonTerminalExpressionDefinition { Identifier = ParserConstants.Identifier },
                    new NonTerminalExpressionDefinition { Identifier = ParserConstants.Join },
                    new NonTerminalExpressionDefinition { Identifier = ParserConstants.Where },
                    new SemanticActionDefinition((ParsingNode node) =>
                    {
                        IdentifierASTNode identifier = node.GetAttributeForKey<IdentifierASTNode>(ParserConstants.Identifier, ParserConstants.SyntaxTreeNode);

                        BooleanExpressionASTNode condition = node.GetAttributeForKey<BooleanExpressionASTNode>(ParserConstants.Where, ParserConstants.SyntaxTreeNode);
                        JoinASTNode join = node.GetAttributeForKey<JoinASTNode>(ParserConstants.Join, ParserConstants.SyntaxTreeNode);

                        FromASTNode from = new FromASTNode()
                        {
                            Identifier = identifier
                        };

                        node.Attributes.Add(ParserConstants.SyntaxTreeNode, new SelectASTNode()
                        {
                            From = from,
                            Condition = condition,
                            Join = join
                        });
                    })
                }
            );
        }

        private static SubProduction GetWhereRule()
        {
            return new SubProduction
            (
                new List<ExpressionDefinition>
                {
                    new TerminalExpressionDefinition { TokenType = TokenType.Where },
                    new NonTerminalExpressionDefinition { Identifier = ParserConstants.BooleanExpression },
                    new SemanticActionDefinition((ParsingNode node) =>
                    {

                        BooleanExpressionASTNode condition = node.GetAttributeForKey<BooleanExpressionASTNode>(ParserConstants.BooleanExpression, ParserConstants.SyntaxTreeNode);

                        node.Attributes.Add(ParserConstants.SyntaxTreeNode, condition);
                    })
                }
            );
        }
    }
}

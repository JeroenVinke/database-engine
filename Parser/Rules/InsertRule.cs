using Compiler.Common;
using Compiler.Parser.SyntaxTreeNodes;
using System;
using System.Collections.Generic;

namespace Compiler.Parser.Rules
{
    public class InsertRule
    {
        public static void Initialize(ref Grammar grammar)
        {
            grammar.Add(new Production(ParserConstants.Select, new List<SubProduction>
            {
                 GetInsertRule()
            }));
        }

        private static SubProduction GetInsertRule()
        {
            return new SubProduction
            (
                new List<ExpressionDefinition>
                {
                    new TerminalExpressionDefinition { TokenType = TokenType.Insert },
                    new TerminalExpressionDefinition { TokenType = TokenType.Into },
                    new NonTerminalExpressionDefinition { Identifier = ParserConstants.Identifier },
                    new TerminalExpressionDefinition { TokenType = TokenType.Values },
                    new TerminalExpressionDefinition { TokenType = TokenType.ParenthesisOpen },
                    new NonTerminalExpressionDefinition { Identifier = ParserConstants.Factors },
                    new TerminalExpressionDefinition { TokenType = TokenType.ParenthesisClose },
                    new SemanticActionDefinition((ParsingNode node) =>
                    {
                        InsertASTNode isnertNode = new InsertASTNode();
                        isnertNode.Into = node.GetAttributeForKey<IdentifierASTNode>("Identifier", ParserConstants.SyntaxTreeNode);
                        isnertNode.Arguments = node.GetAttributeForKey<List<FactorASTNode>>(ParserConstants.Factors, ParserConstants.Factors);

                        node.Attributes.Add(ParserConstants.SyntaxTreeNode, isnertNode);
                    })
                }
            );
        }
    }
}

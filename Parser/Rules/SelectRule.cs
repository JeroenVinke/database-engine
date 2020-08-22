using Compiler.Common;
using Compiler.Parser.SyntaxTreeNodes;
using System.Collections.Generic;

namespace Compiler.Parser.Rules
{
    public class SelectRule
    {
        public static void Initialize(ref Grammar grammar)
        {
            grammar.Add(new Production(ParserConstants.Select, GetSelectRule()));
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
                    new TerminalExpressionDefinition { TokenType = TokenType.Where },
                    new NonTerminalExpressionDefinition { Identifier = ParserConstants.BooleanExpression },
                    new SemanticActionDefinition((ParsingNode node) =>
                    {
                        IdentifierASTNode identifier = node.GetAttributeForKey<IdentifierASTNode>(ParserConstants.Identifier, ParserConstants.SyntaxTreeNode);

                        FromASTNode from = new FromASTNode()
                        {
                            Identifier = identifier
                        };

                        BooleanExpressionASTNode condition = node.GetAttributeForKey<BooleanExpressionASTNode>(ParserConstants.BooleanExpression, ParserConstants.SyntaxTreeNode);

                        node.Attributes.Add(ParserConstants.SyntaxTreeNode, new SelectASTNode()
                        {
                            From = from,
                            Condition = condition
                        });
                    })
                }
            );
        }
    }
}

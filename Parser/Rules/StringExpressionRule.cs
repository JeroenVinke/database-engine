using Compiler.Common;
using Compiler.Parser.SyntaxTreeNodes;
using System;
using System.Collections.Generic;

namespace Compiler.Parser.Rules
{
    public class StringExpressionRule
    {
        public static void Initialize(ref Grammar grammar)
        {
            grammar.Add(new Production(ParserConstants.StringExpression,
                new List<SubProduction>
                {
                    StringRule()
                }
            ));
        }

        private static SubProduction StringRule()
        {
            return new SubProduction
            (
                new List<ExpressionDefinition>
                {
                    new TerminalExpressionDefinition { TokenType = TokenType.String },
                    new SemanticActionDefinition((ParsingNode node) =>
                    {
                        string value = node.GetAttributeForKey<WordToken>(ParserConstants.String, ParserConstants.Token).Lexeme;

                        node.Attributes.Add(ParserConstants.SyntaxTreeNode, new StringASTNode(value) { });
                    })
                }
            );
        }
    }
}
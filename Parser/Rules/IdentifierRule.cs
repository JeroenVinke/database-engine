﻿using Compiler.Common;
using Compiler.Parser.SyntaxTreeNodes;
using System.Collections.Generic;

namespace Compiler.Parser.Rules
{
    public class IdentifierRule
    {
        public static void Initialize(ref Grammar grammar)
        {
            grammar.Add(new Production(ParserConstants.Identifier, GetIdentifierRule()));
        }

        private static SubProduction GetIdentifierRule()
        {
            return new SubProduction
            (
                new List<ExpressionDefinition>
                {
                    new TerminalExpressionDefinition { TokenType = TokenType.Identifier },
                    new SemanticActionDefinition((ParsingNode node) =>
                    {
                        string key = node.GetAttributeForKey<WordToken>(ParserConstants.Identifier, ParserConstants.Token).Lexeme;

                        node.Attributes.Add(ParserConstants.Token, node.GetAttributeForKey<WordToken>(ParserConstants.Identifier, ParserConstants.Token));
                        node.Attributes.Add(ParserConstants.SyntaxTreeNode, new IdentifierASTNode() { Identifier = key });
                    })
                }
            );
        }
    }
}

using System.Collections.Generic;

namespace Compiler.Parser.Rules
{
    public class CommandRule
    {
        public static void Initialize(ref Grammar grammar)
        {
            grammar.Add(new Production(ParserConstants.CommandRule, new List<SubProduction>
            {
                new SubProduction
                (
                    new List<ExpressionDefinition>
                    {
                        new NonTerminalExpressionDefinition { Identifier = ParserConstants.Select },
                        new SemanticActionDefinition((ParsingNode node) =>
                        {
                            node.Attributes.Add(ParserConstants.SyntaxTreeNode, node.GetAttributeForKey<SyntaxTreeNode>(ParserConstants.Select, ParserConstants.SyntaxTreeNode));
                        })
                    }
                ),
                new SubProduction
                (
                    new List<ExpressionDefinition>
                    {
                        new NonTerminalExpressionDefinition { Identifier = ParserConstants.Insert },
                        new SemanticActionDefinition((ParsingNode node) =>
                        {
                            node.Attributes.Add(ParserConstants.SyntaxTreeNode, node.GetAttributeForKey<SyntaxTreeNode>(ParserConstants.Insert, ParserConstants.SyntaxTreeNode));
                        })
                    }
                )
            }));
        }
    }
}

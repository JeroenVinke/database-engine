using Compiler.LexicalAnalyer;
using Compiler.Parser;
using Compiler.Parser.SyntaxTreeNodes;
using DatabaseEngine.Relations;
using System;
using System.Linq;

namespace DatabaseEngine.Commands
{
    public class CommandParser
    {
        private RelationManager _relationManager;

        public CommandParser(RelationManager relationManager)
        {
            _relationManager = relationManager;
        }

        public Command Parse(string query)
        {
            LexicalAnalyzer analyzer = new LexicalAnalyzer(LexicalLanguage.GetLanguage(), query);
            BottomUpParser parser = new BottomUpParser(analyzer);

            parser.Parse();
            parser.OutputDebugFiles();

            SyntaxTreeNode command = parser.TopLevelAST;

            if (command is SelectASTNode selectCommandAST)
            {
                Table table = _relationManager.GetTable(selectCommandAST.From.Identifier.Identifier);

                SelectCommand selectCommand = new SelectCommand
                {
                    Table = table,
                    Condition = BooleanExpressionToCondition(table.TableDefinition, selectCommandAST.Condition),
                    Join = JoinNodeToJoin(table, selectCommandAST.Join)
                };

                return selectCommand;
            }
            else if (command is InsertASTNode insertCommandAST)
            {
                Table table = _relationManager.GetTable(insertCommandAST.Into.Identifier);

                InsertCommand insertCommand = new InsertCommand
                {
                    Table = table,
                    Values = insertCommandAST.Arguments.Select(x => GetValueFromFactor(x)).ToList()
                };

                return insertCommand;
            }

            return null;
        }

        private object GetValueFromFactor(FactorASTNode x)
        {
            if (x is NumberASTNode numberASTNode)
            {
                return numberASTNode.Value;
            }
            else if (x is StringASTNode stringASTNode)
            {
                return stringASTNode.Value;
            }
            else if (x is BooleanASTNode booleanASTNode)
            {
                return booleanASTNode.Value;
            }

            return null;
        }

        private Condition BooleanExpressionToCondition(TableDefinition tableDefinition, BooleanExpressionASTNode expr)
        {
            if (expr is AndASTNode andNode)
            {
                Condition c = new AndCondition()
                {
                    Left = BooleanExpressionToCondition(tableDefinition, andNode.Left),
                    Right = BooleanExpressionToCondition(tableDefinition, andNode.Right),
                };

                return c;
            }
            else if (expr is OrASTNode orNode)
            {
                Condition c = new OrCondition()
                {
                    Left = BooleanExpressionToCondition(tableDefinition, orNode.Left),
                    Right = BooleanExpressionToCondition(tableDefinition, orNode.Right),
                };

                return c;
            }
            else if (expr is RelOpASTNode relopNode
                && relopNode.Left is IdentifierASTNode idAstNode
                && relopNode.Right is StringASTNode stringAstNode)
            {
                return new LeafCondition
                {
                    Column = tableDefinition.First(x => x.Name.ToLower() == idAstNode.Identifier.ToLower()),
                    Operation = relopNode.RelationOperator,
                    Value = stringAstNode.Value
                };
            }

            return null;
        }

        private Join JoinNodeToJoin(Table leftTable, JoinASTNode join)
        {
            if (join == null)
            {
                return null;
            }

            Table rightTable = _relationManager.GetTable(join.TargetTable.Identifier);

            return new Join
            {
                LeftTable = leftTable,
                RightTable = rightTable,
                LeftColumn = leftTable.TableDefinition.GetAttributeByName(join.LeftColumn.Identifier.Split(".")[1]),
                RightColumn = rightTable.TableDefinition.GetAttributeByName(join.RightColumn.Identifier.Split(".")[1])
            };
        }

    }
}

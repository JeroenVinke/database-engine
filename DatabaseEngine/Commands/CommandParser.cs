using Compiler.Common;
using Compiler.LexicalAnalyer;
using Compiler.Parser;
using Compiler.Parser.SyntaxTreeNodes;
using DatabaseEngine.Relations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine.Commands
{
    public class CommandParser
    {
        private RelationManager _relationManager;

        public BottomUpParser Parser { get; set; }

        public CommandParser(RelationManager relationManager)
        {
            _relationManager = relationManager;
            Parser = new BottomUpParser();
        }

        public Command Parse(string query)
        {
            LexicalAnalyzer analyzer = new LexicalAnalyzer(LexicalLanguage.GetLanguage(), query);
            Parser.Parse(analyzer);

            SyntaxTreeNode command = Parser.TopLevelAST;

            if (command is SelectASTNode selectCommandAST)
            {
                Table table = _relationManager.GetTable(selectCommandAST.From.Identifier.Identifier);

                SelectCommand selectCommand = new SelectCommand
                {
                    Table = table,
                    Condition = BooleanExpressionToCondition(table.TableDefinition, selectCommandAST.Condition),
                    Join = JoinNodeToJoin(table, selectCommandAST.Join),
                    Columns = SelectColumnsToColumns(table, selectCommandAST.SelectColumns),
                    Top = selectCommandAST?.Top?.Amount
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

        private List<AttributeDefinition> SelectColumnsToColumns(Table table, List<FactorASTNode> selectColumns)
        {
            List<AttributeDefinition> result = new List<AttributeDefinition>();

            foreach (FactorASTNode factor in selectColumns)
            {
                if (factor is IdentifierASTNode identifierASTNode)
                {
                    if (identifierASTNode.Identifier == "*")
                    {
                        result.Add(new AttributeDefinition() { Name = "*" });
                    }
                    else
                    {
                        if (identifierASTNode.Identifier.Contains("."))
                        {
                            result.Add(GetColumnFromJoinString(GetTableFromJoinString(identifierASTNode.Identifier), identifierASTNode.Identifier));
                        }
                        else
                        {
                            result.Add(table.TableDefinition.GetAttributeByName(identifierASTNode.Identifier));
                        }
                    }
                }
            }

            return result;
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
            else if (expr is RelOpASTNode relopNode)
            {
                string column = "";

                if (relopNode.Left is IdentifierASTNode)
                {
                    column = ((IdentifierASTNode)relopNode.Left).Identifier;
                }
                else if (relopNode.Right is IdentifierASTNode)
                {
                    column = ((IdentifierASTNode)relopNode.Right).Identifier;
                }

                object value = GetValueFromConditionASTNode(relopNode.Right) ?? GetValueFromConditionASTNode(relopNode.Left);

                return new LeafCondition
                {
                    Column = GetColumnFromJoinString(column.Contains(".") ? GetTableFromJoinString(column) : _relationManager.GetTable(tableDefinition.Name), column),
                    Operation = relopNode.RelationOperator,
                    Value = value
                };
            }

            return null;
        }

        private object GetValueFromConditionASTNode(FactorASTNode node)
        {
            if (node is StringASTNode stringASTNode)
            {
                return stringASTNode.Value;
            }
            else if (node is NumberASTNode intASTNode)
            {
                return intASTNode.Value;
            }

            return null;
        }

        private Join JoinNodeToJoin(Table joinTable, JoinASTNode join)
        {
            if (join == null)
            {
                return null;
            }

            Table leftTable = GetTableFromJoinString(join.LeftColumn.Identifier);
            Table rightTable = GetTableFromJoinString(join.RightColumn.Identifier);
            AttributeDefinition leftColumn = GetColumnFromJoinString(leftTable, join.LeftColumn.Identifier);
            AttributeDefinition rightColumn = GetColumnFromJoinString(rightTable, join.RightColumn.Identifier);

            bool doSwitch = leftTable != joinTable;

            return new Join
            {
                LeftTable = doSwitch ? rightTable : leftTable,
                LeftColumn = doSwitch ? rightColumn : leftColumn,
                RightTable = doSwitch ? leftTable : rightTable,
                RightColumn = doSwitch ? leftColumn : rightColumn,
            };
        }

        private Table GetTableFromJoinString(string joinString)
        {
            return _relationManager.GetTable(joinString.Split(".")[0]);
        }

        private AttributeDefinition GetColumnFromJoinString(Table table, string joinString)
        {
            return table.TableDefinition.GetAttributeByName(joinString.Contains(".") ? joinString.Split(".")[1] : joinString);
        }
    }
}

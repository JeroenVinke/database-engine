using Compiler.LexicalAnalyer;
using Compiler.Parser;
using Compiler.Parser.SyntaxTreeNodes;
using DatabaseEngine.Relations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine.LogicalPlan
{
    public class LogicalQueryPlan
    {
        private RelationManager _relationManager;
        public BottomUpParser Parser { get; set; }

        public LogicalQueryPlan(RelationManager relationManager)
        {
            _relationManager = relationManager;
            Parser = new BottomUpParser();
        }

        public LogicalElement GetTreeForQuery(string query)
        {
            LexicalAnalyzer analyzer = new LexicalAnalyzer(LexicalLanguage.GetLanguage(), query);
            Parser.Parse(analyzer);

            SyntaxTreeNode command = Parser.TopLevelAST;

            return GetElementForTreeNode(command);
        }

        private LogicalElement GetElementForTreeNode(SyntaxTreeNode node)
        {
            if (node is SelectASTNode selectASTNode)
            {
                Table table = _relationManager.GetTable(selectASTNode.From.Identifier.Identifier);

                LogicalElement result = new RelationElement(table.TableDefinition);

                if (selectASTNode.Join != null)
                {
                    Join join = JoinNodeToJoin(table.TableDefinition, selectASTNode.Join);

                    RelationElement joinRelation = new RelationElement(join.RightTable);

                    result = new CartesianProductElement(result, joinRelation, join.LeftColumn, join.RightColumn);
                }

                if (selectASTNode.Condition != null)
                {
                    // todo: IN (1,2,3)
                    if (TryOptimizeIN(selectASTNode, result, table.TableDefinition, out CartesianProductElement productElement))
                    {
                        result = productElement;
                    }
                    else
                    {
                        Condition condition = BooleanExpressionToCondition(table.TableDefinition, selectASTNode.Condition);

                        result = new FilterElement(result, condition);
                    }
                }

                result = new ProjectionElement(result, SelectColumnsToColumns(table, selectASTNode.SelectColumns));

                return result;
            }
            else if (node is InsertASTNode insertASTNode)
            {
                Table table = _relationManager.GetTable(insertASTNode.Into.Identifier);
                Set set = new Set(table.TableDefinition);
                set.Add(insertASTNode.Arguments.Select(x => GetValueFromFactor(x)).ToArray());

                return new InsertElement(table.TableDefinition, new MemorySetElement(table.TableDefinition, set));
            }

            return null;
        }

        private bool TryOptimizeIN(SelectASTNode selectASTNode, LogicalElement input, TableDefinition tableDefinition, out CartesianProductElement result)
        {
            if(selectASTNode.Condition is RelOpASTNode relOpASTNode
                && relOpASTNode.RelationOperator == Compiler.Common.RelOp.In)
            {
                SelectASTNode innerSelectASTNode = null;
                IdentifierASTNode outerIdentifierASTNode = null;

                if (relOpASTNode.Right is SelectASTNode)
                {
                    innerSelectASTNode = (SelectASTNode)relOpASTNode.Right;

                    if (relOpASTNode.Left is IdentifierASTNode)
                    {
                        outerIdentifierASTNode = (IdentifierASTNode)relOpASTNode.Left;
                    }
                }

                if (relOpASTNode.Right is IdentifierASTNode)
                {
                    outerIdentifierASTNode = (IdentifierASTNode)relOpASTNode.Right;

                    if (relOpASTNode.Left is SelectASTNode)
                    {
                        innerSelectASTNode = (SelectASTNode)relOpASTNode.Left;
                    }
                }

                if (innerSelectASTNode != null
                    && outerIdentifierASTNode != null)
                {
                    result = new CartesianProductElement(input, GetElementForTreeNode(innerSelectASTNode), GetColumnFromIdentifierNode(tableDefinition, outerIdentifierASTNode), SelectColumnsToColumns(_relationManager.GetTable(innerSelectASTNode.From.Identifier.Identifier), innerSelectASTNode.SelectColumns).First().AttributeDefinition);
                    return true;
                }
            }

            result = null;
            return false;
        }

        private List<ProjectionColumn> SelectColumnsToColumns(Table table, List<FactorASTNode> selectColumns)
        {
            List<ProjectionColumn> result = new List<ProjectionColumn>();

            foreach (FactorASTNode factor in selectColumns)
            {
                if (factor is IdentifierASTNode identifierASTNode)
                {
                    if (identifierASTNode.Identifier == "*")
                    {
                        result.Add(new ProjectionColumn { Relation = table.TableDefinition, AttributeDefinition = new AttributeDefinition() { Name = "*" } });
                    }
                    else
                    {
                        if (identifierASTNode.Identifier.Contains("."))
                        {
                            TableDefinition tableDefinition = GetTableDefinitionFromJoinString(identifierASTNode.Identifier);
                            result.Add(new ProjectionColumn { Relation = tableDefinition, AttributeDefinition = GetColumnFromJoinString(tableDefinition, identifierASTNode.Identifier) });
                        }
                        else
                        {
                            result.Add(new ProjectionColumn { Relation = table.TableDefinition, AttributeDefinition = table.TableDefinition.GetAttributeByName(identifierASTNode.Identifier) });
                        }
                    }
                }
            }

            return result;
        }

        private TableDefinition GetTableDefinitionFromJoinString(string joinString)
        {
            return _relationManager.GetTable(joinString.Split(".")[0]).TableDefinition;
        }

        private AttributeDefinition GetColumnFromJoinString(TableDefinition table, string joinString)
        {
            return table.GetAttributeByName(joinString.Contains(".") ? joinString.Split(".")[1] : joinString);
        }

        private Join JoinNodeToJoin(TableDefinition joinTable, JoinASTNode join)
        {
            if (join == null)
            {
                return null;
            }

            TableDefinition leftTable = GetTableDefinitionFromJoinString(join.LeftColumn.Identifier);
            TableDefinition rightTable = GetTableDefinitionFromJoinString(join.RightColumn.Identifier);
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
                AttributeDefinition attributeDefinition = null;

                if (relopNode.Left is IdentifierASTNode)
                {
                    attributeDefinition = GetColumnFromIdentifierNode(tableDefinition, (IdentifierASTNode)relopNode.Left);
                }
                else if (relopNode.Right is IdentifierASTNode)
                {
                    attributeDefinition = GetColumnFromIdentifierNode(tableDefinition, (IdentifierASTNode)relopNode.Right);
                }

                object value = GetValueFromConditionASTNode(relopNode.Right) ?? GetValueFromConditionASTNode(relopNode.Left);

                return new LeafCondition
                {
                    Column = attributeDefinition,
                    Operation = relopNode.RelationOperator,
                    Value = value
                };
            }

            return null;
        }

        private AttributeDefinition GetColumnFromIdentifierNode(TableDefinition tableDefinition, IdentifierASTNode relopNode)
        {
            string column = relopNode.Identifier;

            return GetColumnFromJoinString(column.Contains(".") ? GetTableDefinitionFromJoinString(column) : _relationManager.GetTable(tableDefinition.Name).TableDefinition, column);
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
            else if (node is SelectASTNode selectASTNode)
            {
                return GetElementForTreeNode(selectASTNode);
            }

            return null;
        }
    }
}


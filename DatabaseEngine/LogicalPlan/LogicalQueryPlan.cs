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

                ReadLogicalElement result = new RelationElement(table.TableDefinition);

                Condition condition = null;

                if (selectASTNode.Condition != null)
                {
                    condition = BooleanExpressionToCondition(table.TableDefinition, selectASTNode.Condition);
                    //// todo: IN (1,2,3)
                    //// todo: IN (select id from products) AND x = 1
                    //if (TryOptimizeIN(selectASTNode, result, table.TableDefinition, out CartesianProductElement productElement))
                    //{
                    //    result = productElement;
                    //}
                    //else
                    //{
                    //}
                }

                if (selectASTNode.Join != null)
                {
                    Join join = JoinNodeToJoin(table.TableDefinition, selectASTNode.Join);

                    ReadLogicalElement joinRelation = new RelationElement(join.RightTable);

                    (Condition leftPushedDownCondition, Condition leftover1) = TryPushdown((RelationElement)result, condition);
                    (Condition rightPushedDownCondition, Condition leftover2) = TryPushdown((RelationElement)joinRelation, leftover1);
                    leftPushedDownCondition = leftPushedDownCondition.Simplify();
                    rightPushedDownCondition = rightPushedDownCondition.Simplify();
                    condition = leftover2?.Simplify();

                    if (leftPushedDownCondition != null)
                    {
                        result = new SelectionElement(result, leftPushedDownCondition);
                    }
                    if (rightPushedDownCondition != null)
                    {
                        joinRelation = new SelectionElement(joinRelation, rightPushedDownCondition);
                    }

                    result = new CartesianProductElement(result, joinRelation, join.LeftColumn, join.RightColumn);
                }
                else
                {
                    result = new RelationElement(table.TableDefinition);
                }


                result = new SelectionElement(result, condition);
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

        private (Condition, Condition) TryPushdown(RelationElement input, Condition condition, Condition pushedDownCondition = null)
        {
            if (condition == null)
            {
                return (pushedDownCondition, condition);
            }

            if (pushedDownCondition == null)
            {
                pushedDownCondition = new AndCondition();
            }

            if (condition is AndCondition and)
            {
                if (and.Left is LeafCondition leftLeaf)
                {
                    if (leftLeaf.Column.Relation.Id == input.Relation.Id)
                    {
                        pushedDownCondition = new AndCondition()
                        {
                            Left = pushedDownCondition,
                            Right = leftLeaf
                        };

                        and.Left = null;
                    }
                }
                else
                {
                    return TryPushdown(input, and.Left, pushedDownCondition);
                }

                if (and.Right is LeafCondition rightLeaf)
                {
                    if (rightLeaf.Column.Relation.Id == input.Relation.Id)
                    {
                        pushedDownCondition = new AndCondition()
                        {
                            Left = pushedDownCondition,
                            Right = rightLeaf
                        };

                        and.Right = null;
                    }
                }
                else
                {
                    return TryPushdown(input, and.Right, pushedDownCondition);
                }
            }
            else if (condition is LeafCondition leaf)
            {
                if (leaf.Column.Relation.Id == input.Relation.Id)
                {
                    pushedDownCondition = new AndCondition()
                    {
                        Left = pushedDownCondition,
                        Right = leaf
                    };

                    condition = null;
                }
            }

            return (pushedDownCondition, condition);
        }

        private bool TryOptimizeIN(SelectASTNode selectASTNode, ReadLogicalElement input, TableDefinition tableDefinition, out CartesianProductElement result)
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
                    result = new CartesianProductElement(input, GetElementForTreeNode(innerSelectASTNode) as ReadLogicalElement, GetColumnFromIdentifierNode(tableDefinition, outerIdentifierASTNode), SelectColumnsToColumns(_relationManager.GetTable(innerSelectASTNode.From.Identifier.Identifier), innerSelectASTNode.SelectColumns).First().AttributeDefinition);
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

        private Condition BooleanExpressionToCondition(TableDefinition mainTable, BooleanExpressionASTNode expr)
        {
            if (expr is AndASTNode andNode)
            {
                Condition c = new AndCondition()
                {
                    Left = BooleanExpressionToCondition(mainTable, andNode.Left),
                    Right = BooleanExpressionToCondition(mainTable, andNode.Right),
                };

                return c;
            }
            else if (expr is OrASTNode orNode)
            {
                Condition c = new OrCondition()
                {
                    Left = BooleanExpressionToCondition(mainTable, orNode.Left),
                    Right = BooleanExpressionToCondition(mainTable, orNode.Right),
                };

                return c;
            }
            else if (expr is RelOpASTNode relopNode)
            {
                AttributeDefinition attributeDefinition = null;

                if (relopNode.Left is IdentifierASTNode)
                {
                    attributeDefinition = GetColumnFromIdentifierNode(mainTable, (IdentifierASTNode)relopNode.Left);
                }
                else if (relopNode.Right is IdentifierASTNode)
                {
                    attributeDefinition = GetColumnFromIdentifierNode(mainTable, (IdentifierASTNode)relopNode.Right);
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


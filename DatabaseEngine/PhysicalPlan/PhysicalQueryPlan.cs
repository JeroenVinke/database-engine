using DatabaseEngine.LogicalPlan;
using DatabaseEngine.PhysicalPlan;
using DatabaseEngine.Relations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine.Operations
{
    public class PhysicalQueryPlan
    {
        private RelationManager _relationManager;
        private StatisticsManager _statisticsManager;

        public PhysicalQueryPlan(RelationManager relationManager, StatisticsManager statisticsManager)
        {
            _relationManager = relationManager;
            _statisticsManager = statisticsManager;
        }

        internal PhysicalOperation CreateFromPath(LogicalElement logicalTree, List<QueryPlanNode> path)
        {
            ApplyInputOperations(logicalTree, path);

            return path.First(x => x.LogicalElement == logicalTree).PhysicalOperation;
        }

        private void ApplyInputOperations(LogicalElement logicalTree, List<QueryPlanNode> path)
        {
            if (logicalTree == null)
            {
                return;
            }

            PhysicalOperation curNode = path.First(x => x.LogicalElement == logicalTree).PhysicalOperation;
            PhysicalOperation leftInput = null;
            if (logicalTree.LeftChild != null)
            {
                leftInput = path.First(x => x.LogicalElement == logicalTree.LeftChild).PhysicalOperation;
            }

            PhysicalOperation rightInput = null;
            if (logicalTree.RightChild != null)
            {
                rightInput = path.First(x => x.LogicalElement == logicalTree.RightChild).PhysicalOperation;
            }

            curNode?.SetInput(leftInput, rightInput);

            ApplyInputOperations(logicalTree.LeftChild, path);
            ApplyInputOperations(logicalTree.RightChild, path);
        }

        //private PhysicalOperation FindIndexes(SelectionElement selectionElement)
        //{
        //    List<LeafCondition> leafs = GetLeafConditions(selectionElement.Condition);

        //    foreach(LeafCondition leaf in leafs)
        //    {
        //        if (leaf.Column.Relation is TableDefinition tableDefinition)
        //        {
        //            Index index = tableDefinition.Indexes.FirstOrDefault(x => string.Equals(x.Column, leaf.Column.Name, StringComparison.OrdinalIgnoreCase));

        //            if (index != null)
        //            {
        //                ;
        //            }
        //        }
        //    }

        //    if (leafs.Count == 0)
        //    {
        //        // index seek / table scan
        //        return GetFullTableScanOperation(selectionElement..)
        //    }
        //}

        public PhysicalOperation GetPhysicalPlan(LogicalElement logicalTree)
        {
            List<LogicalElement> postorderLogicalTree = AppendPostOrder(new List<LogicalElement>(), logicalTree);

            QueryPlanNode root = new QueryPlanNode(null, null);

            List<QueryPlanNode> lastOptions = new List<QueryPlanNode> { root };

            for (int i = 0; i < postorderLogicalTree.Count; i++)
            {
                LogicalElement element = postorderLogicalTree[i];

                Dictionary<QueryPlanNode, int> options = GetOptions(element);
                ConnectEndNodesToNodes(lastOptions, options);

                lastOptions = options.Select(x => x.Key).ToList();
            }

            List<QueryPlanNode> leastCostPath = GetLeastCostPath(root);

            PhysicalOperation physicalOperation = CreateFromPath(logicalTree, leastCostPath);
            return physicalOperation;
        }

        private List<LeafCondition> GetLeafConditions(Condition condition)
        {
            List<LeafCondition> c = new List<LeafCondition>();

            if (condition is AndCondition and)
            {
                c.AddRange(GetLeafConditions(and.Left));
                c.AddRange(GetLeafConditions(and.Right));
            }
            else if (condition is OrCondition or)
            {
                c.AddRange(GetLeafConditions(or.Left));
                c.AddRange(GetLeafConditions(or.Right));
            }
            else if (condition is LeafCondition leaf)
            {
                c.Add(leaf);
            }

            return c;
        }


        private void ConnectEndNodesToNodes(IEnumerable<QueryPlanNode> nodes, Dictionary<QueryPlanNode, int> endNodes)
        {
            foreach (QueryPlanNode node in nodes)
            {
                ConnectEndNodesToNodes(node, endNodes);
            }
        }

        private void ConnectEndNodesToNodes(QueryPlanNode node, Dictionary<QueryPlanNode, int> endNodes)
        {
            if (node.Edges.Count == 0)
            {
                foreach (KeyValuePair<QueryPlanNode, int> endNode in endNodes)
                {
                    node.Edges.Add(new QueryPlanNodeEdge
                    {
                        From = node,
                        To = endNode.Key,
                        Cost = endNode.Value
                    });
                }
            }
            else
            {

                foreach (QueryPlanNode node1 in node.Edges.Select(x => x.To))
                {
                    ConnectEndNodesToNodes(node1, endNodes);
                }
            }
        }

        private Dictionary<QueryPlanNode, int> GetOptions(LogicalElement element)
        {
            Dictionary<QueryPlanNode, int> options = new Dictionary<QueryPlanNode, int>();

            if (element is ProjectionElement p)
            {
                PhysicalOperation proj = new ProjectionOperation(element, null, p.Columns.Select(x => x.AttributeDefinition).ToList());
                options.Add(new QueryPlanNode(element, proj), proj.GetCost());
            }
            else if (element is SelectionElement selectionElement)
            {
                if (selectionElement.LeftChild is RelationElement relationElement)
                {
                    Table table = Program.RelationManager.GetTable(relationElement.Relation.Id);

                    PhysicalOperation tableScan = new TableScanOperation(element, table);
                    if (selectionElement.Condition != null)
                    {
                        PhysicalOperation f = new FilterOperation(element, tableScan, selectionElement.Condition);
                        options.Add(new QueryPlanNode(element, f), f.GetCost());
                    }
                    else
                    {
                        options.Add(new QueryPlanNode(element, tableScan), tableScan.GetCost());
                    }

                    Condition clonedCondition = selectionElement.Condition?.Clone();

                    if (TryExtractConstantConditionWithIndex(relationElement.Relation, clonedCondition, out LeafCondition constantCondition))
                    {
                        selectionElement.Condition = selectionElement.Condition.Simplify();

                        PhysicalOperation indexSeek = new IndexSeekOperation(element, table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column), constantCondition);

                        if (selectionElement.Condition != null)
                        {
                            PhysicalOperation f = new FilterOperation(element, indexSeek, clonedCondition);
                            options.Add(new QueryPlanNode(element, f), f.GetCost());
                        }
                        else
                        {
                            options.Add(new QueryPlanNode(element, indexSeek), indexSeek.GetCost());
                        }
                    }
                    else
                    {
                        if (table.TableDefinition.HasClusteredIndex())
                        {
                            PhysicalOperation indexSeek = new IndexSeekOperation(element, table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column), constantCondition);

                            if (selectionElement.Condition != null)
                            {
                                PhysicalOperation f = new FilterOperation(element, indexSeek, selectionElement.Condition);
                                options.Add(new QueryPlanNode(element, f), f.GetCost());
                            }
                            else
                            {
                                options.Add(new QueryPlanNode(element, indexSeek), indexSeek.GetCost());
                            }
                        }
                    }
                }
            }
            else if (element is RelationElement relElement)
            {
                Table table = Program.RelationManager.GetTable(relElement.Relation.Id);

                if (table.TableDefinition.HasClusteredIndex())
                {
                    PhysicalOperation indexSeek = new IndexScanOperation(element, table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column));
                    options.Add(new QueryPlanNode(element, indexSeek), indexSeek.GetCost());
                }

                PhysicalOperation tableScan = new TableScanOperation(element, table);
                options.Add(new QueryPlanNode(element, tableScan), tableScan.GetCost());
            }
            else if (element is CartesianProductElement cartesianProduct)
            {
                PhysicalOperation left = null;//GetDefaultPhysicalOperation(element.LeftChild);
                PhysicalOperation right = null;//GetDefaultPhysicalOperation(element.RightChild);

                PhysicalOperation join = new NestedLoopJoinOperation(cartesianProduct, (ReadLogicalElement)element.LeftChild, (ReadLogicalElement)element.RightChild, cartesianProduct.LeftJoinColumn, cartesianProduct.RightJoinColumn);
                options.Add(new QueryPlanNode(cartesianProduct, join), join.GetCost());
            }

            if (options.Count == 0)
            {
                throw new Exception();
            }

            return options;
        }

        private List<LogicalElement> AppendPostOrder(List<LogicalElement> result, LogicalElement center)
        {
            if (center.LeftChild != null)
            {
                AppendPostOrder(result, center.LeftChild);
            }
            if (center.RightChild != null)
            {
                AppendPostOrder(result, center.RightChild);
            }
            result.Add(center);

            return result;
        }


        private List<QueryPlanNode> GetLeastCostPath(QueryPlanNode root)
        {
            QueryPlanNode cur = root;
            List<QueryPlanNode> result = new List<QueryPlanNode>();
            result.Add(cur);

            while (cur != null)
            {
                QueryPlanNodeEdge cheapestEdge = cur.Edges.OrderBy(x => x.Cost).FirstOrDefault();
                if (cheapestEdge != null)
                {
                    cur = cheapestEdge.To;

                    if (cur != null)
                    {
                        result.Add(cur);
                    }
                }
                else
                {
                    cur = null;
                }
            }

            return result;
        }

        private bool TryExtractConstantConditionWithIndex(Relation relation, Condition condition, out LeafCondition result)
        {
            result = null;

            if (condition is AndCondition andCondition)
            {
                if (TryExtractConstantConditionWithIndex(relation, andCondition.Left, out result))
                {
                    return true;
                }
                else if (TryExtractConstantConditionWithIndex(relation, andCondition.Right, out result))
                {
                    return true;
                }

                return false;
            }
            else if (condition is LeafCondition leaf
                && leaf.Operation == Compiler.Common.RelOp.Equals)
            {
                foreach (Index index in (relation as TableDefinition).Indexes)
                {
                    if (leaf.Column == (relation as TableDefinition).GetAttributeByName(index.Column))
                    {
                        result = new LeafCondition()
                        {
                            Column = leaf.Column,
                            Operation = leaf.Operation,
                            Value = leaf.Value
                        };

                        leaf.AlwaysTrue = true;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}

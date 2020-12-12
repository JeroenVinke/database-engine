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

            PhysicalOperation curNode = logicalTree != null ? path.First(x => x.LogicalElement == logicalTree)?.PhysicalOperation : null;
            PhysicalOperation leftInput = logicalTree.LeftChild != null ? path.First(x => x.LogicalElement == logicalTree.LeftChild)?.PhysicalOperation : null;
            PhysicalOperation rightInput = logicalTree.RightChild != null ? path.First(x => x.LogicalElement == logicalTree.RightChild)?.PhysicalOperation : null;
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


        // todo: based on statistics, try different phyiscal plans
        private PhysicalOperation GetDefaultPhysicalOperation(PhysicalOperation input, LogicalElement element)
        {
            if (element is MemorySetElement memorySetElement)
            {
                return new MemorySetOperation(element, memorySetElement.Set);
            }
            else if (element is ProjectionElement projectionElement)
            {
                return new ProjectionOperation(element, GetDefaultPhysicalOperation(input, projectionElement.LeftChild), projectionElement.Columns.Select(x => x.AttributeDefinition).ToList());
            }
            else if (element is RelationElement relationElement)
            {
                Table table = _relationManager.GetTable(relationElement.Relation.Id);

                if (table.TableDefinition.HasClusteredIndex())
                {
                    return new IndexScanOperation(element, table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column));
                }
                return new TableScanOperation(element, table);
            }

            return null;
        }
    }
}

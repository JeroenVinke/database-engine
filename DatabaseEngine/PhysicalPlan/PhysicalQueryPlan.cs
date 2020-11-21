using DatabaseEngine.LogicalPlan;
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

        public PhysicalOperation GetFromLogicalTree(LogicalElement logicalTree)
        {
            if (logicalTree is InsertElement insertElement)
            {
                return new InsertOperation(_relationManager.GetTable(insertElement.TableDefinition.Id), GetDefaultPhysicalOperation(insertElement.LeftChild));
            }
            return GetDefaultPhysicalOperation(logicalTree as ReadLogicalElement);
        }

        private PhysicalOperation GetDefaultPhysicalOperation(LogicalElement element)
        {
            //PhysicalOperation left = null;
            //if (element.LeftChild != null)
            //{
            //    left = GetDefaultPhysicalOperation(element.LeftChild);
            //}

            //PhysicalOperation right = null;
            //if (element.RightChild != null)
            //{
            //    right = GetDefaultPhysicalOperation(element.RightChild);
            //}

            if (element is MemorySetElement memorySetElement)
            {
                return new MemorySetOperation(memorySetElement.Set);
            }
            else if (element is ProjectionElement projectionElement)
            {
                return new ProjectionOperation(GetDefaultPhysicalOperation(projectionElement.LeftChild), projectionElement.Columns.Select(x => x.AttributeDefinition).ToList());
            }
            else if (element is RelationElement relationElement)
            {
                Table table = _relationManager.GetTable(relationElement.Relation.Id);

                if (table.TableDefinition.HasClusteredIndex())
                {
                    return new IndexScanOperation(table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column));
                }
                return new TableScanOperation(table);
            }
            else if (element is SelectionElement selectionElement)
            {
                //Table table = _relationManager.GetTable(selectionElement.Relation.Id);

                //PhysicalOperation input = null;
                //if (table.TableDefinition.HasClusteredIndex())
                //{
                //    input = new IndexSeekOperation(table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column));
                //}
                //else
                //{
                //    input = new TableScanOperation(table);
                //}

                PhysicalOperation input = GetDefaultPhysicalOperation(selectionElement.LeftChild);

                // has index
                if (selectionElement.LeftChild is RelationElement relElement && relElement.Relation.Name.ToLower() == "products")
                {
                    return new IndexSeekOperation(Program.RelationManager.GetTable(relElement.Relation.Id), Program.RelationManager.GetTable(relElement.Relation.Id).GetIndex((relElement.Relation as TableDefinition).GetClusteredIndex().Column), selectionElement.Condition);
                }

                return new FilterOperation(input, selectionElement.Condition);
            }
            else if (element is CartesianProductElement cartesianProductElement)
            {
                PhysicalOperation left = GetDefaultPhysicalOperation(element.LeftChild);
                PhysicalOperation right = GetDefaultPhysicalOperation(element.RightChild);

                //int leftSize = left.EstimateNumberOfRows();
                //int rightSize = right.EstimateNumberOfRows();

                //int x = _statisticsManager.GetSizeOfCondition((right.InputOperations.First() as TableScanOperation).Table.TableDefinition, (right as FilterOperation).Condition);

                return new NestedLoopJoinOperation(left, right, cartesianProductElement.LeftJoinColumn, cartesianProductElement.RightJoinColumn);
            }

            return null;
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
                return new MemorySetOperation(memorySetElement.Set);
            }
            else if (element is ProjectionElement projectionElement)
            {
                return new ProjectionOperation(GetDefaultPhysicalOperation(input, projectionElement.LeftChild), projectionElement.Columns.Select(x => x.AttributeDefinition).ToList());
            }
            else if (element is RelationElement relationElement)
            {
                Table table = _relationManager.GetTable(relationElement.Relation.Id);

                if (table.TableDefinition.HasClusteredIndex())
                {
                    return new IndexScanOperation(table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column));
                }
                return new TableScanOperation(table);
            }

            return null;
        }
    }
}

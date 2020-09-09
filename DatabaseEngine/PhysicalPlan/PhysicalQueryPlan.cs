using DatabaseEngine.LogicalPlan;
using DatabaseEngine.Relations;
using System;
using System.Linq;

namespace DatabaseEngine.Operations
{
    public class PhysicalQueryPlan
    {
        private RelationManager _relationManager;

        public PhysicalQueryPlan(RelationManager relationManager)
        {
            _relationManager = relationManager;
        }

        public PhysicalOperation GetFromLogicalTree(LogicalElement logicalTree)
        {
            if (logicalTree is InsertElement insertElement)
            {
                return new InsertOperation(_relationManager.GetTable(insertElement.TableDefinition.Id), GetDefaultPhysicalOperation(insertElement.LeftChild));
            }
            return GetDefaultPhysicalOperation(logicalTree);
        }

        private PhysicalOperation GetDefaultPhysicalOperation(LogicalElement element)
        {
            PhysicalOperation left = null;
            if (element.LeftChild != null)
            {
                left = GetDefaultPhysicalOperation(element.LeftChild);
            }

            PhysicalOperation right = null;
            if (element.RightChild != null)
            {
                right = GetDefaultPhysicalOperation(element.RightChild);
            }

            if (element is MemorySetElement memorySetElement)
            {
                return new MemorySetOperation(memorySetElement.Set);
            }
            else if (element is ProjectionElement projectionElement)
            {
                return new ProjectionOperation(GetDefaultPhysicalOperation( projectionElement.LeftChild), projectionElement.Columns.Select(x => x.AttributeDefinition).ToList());
            }
            else if (element is RelationElement relationElement)
            {
                Table table = _relationManager.GetTable(relationElement.Relation.Id);

                if (table.TableDefinition.HasClusteredIndex())
                {
                    return new IndexSeekOperation(table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column));
                }
                return new TableScanOperation(table);
            }
            else if (element is FilterElement filterElement)
            {
                return new FilterOperation(left, filterElement.Condition);
            }
            else if (element is CartesianProductElement cartesianProductElement)
            {
                return new NestedLoopJoinOperation(left, right, cartesianProductElement.LeftJoinColumn, cartesianProductElement.RightJoinColumn);
            }

            return null;
        }


        // todo: based on statistics, try different phyiscal plans
        //    private PhysicalOperation GetDefaultPhysicalOperation(PhysicalOperation input, LogicalElement element)
        //    {
        //        if (element is MemorySetElement memorySetElement)
        //        {
        //            return new MemorySetOperation(memorySetElement.Set);
        //        }
        //        else if (element is ProjectionElement projectionElement)
        //        {
        //            return new ProjectionOperation(GetDefaultPhysicalOperation(input, projectionElement.LeftChild), projectionElement.Columns.Select(x => x.AttributeDefinition).ToList());
        //        }
        //        else if (element is RelationElement relationElement)
        //        {
        //            Table table = _relationManager.GetTable(relationElement.Relation.Id);

        //            if (table.TableDefinition.HasClusteredIndex())
        //            {
        //                return new IndexSeekOperation(table, table.GetIndex(table.TableDefinition.GetClusteredIndex().Column));
        //            }
        //            return new TableScanOperation(table);
        //        }
        //        else if (element is FilterElement filterElement)
        //        {
        //            return new FilterOperation(input, filterElement.Condition);
        //        }

        //        return null;
        //    }
        //}

        //public Operation GetFullTableScanOperation(Table table, AttributeDefinition column = null)
        //{
        //    if (table.TableDefinition.HasClusteredIndex())
        //    {
        //        return new IndexSeekOperation(table, table.GetIndex(column?.Name ?? table.TableDefinition.GetClusteredIndex().Column));
        //    }
        //    else
        //    {
        //        return new TableScanOperation(table);
        //    }
        //}
    }
}

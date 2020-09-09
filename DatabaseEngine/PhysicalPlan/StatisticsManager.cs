using DatabaseEngine.LogicalPlan;
using DatabaseEngine.Operations;
using DatabaseEngine.Relations;
using DatabaseEngine.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class StatisticsManager
    {
        private Dictionary<int, TableStatistics> _statisticsPerRelation = new Dictionary<int, TableStatistics>();
        private RelationManager _relationManager;

        public StatisticsManager(RelationManager relationManager)
        {
            _relationManager = relationManager;
        }

        public TableStatistics GetStatistics(int relationId)
        {
            if (_statisticsPerRelation.TryGetValue(relationId, out TableStatistics foundStatistics))
            {
                return foundStatistics;
            }

            TableStatistics statistics = new TableStatistics() { RelationId = relationId };
            _statisticsPerRelation.Add(relationId, statistics);

            return statistics;
        }

        public void CalculateStatistics()
        {
            foreach (Table table in _relationManager.Tables)
            {
                TableStatistics statistics = GetStatistics(table.TableDefinition.Id);
                statistics.TotalSize = GetTotalSizeEstimate(table.TableDefinition);
            }
        }

        public int GetSizeOfCondition(TableDefinition tableDefinition, Condition condition)
        {
            int sizeOfRelation = GetTotalSizeEstimate(tableDefinition);
            if (condition is AndCondition andCondition)
            {
                return sizeOfRelation * ((GetSizeOfCondition(tableDefinition, andCondition.Left) / sizeOfRelation) * (GetSizeOfCondition(tableDefinition, andCondition.Right) / sizeOfRelation));
            }
            if (condition is OrCondition orCondition)
            {
                int m1 = GetSizeOfCondition(tableDefinition, orCondition.Left);
                int m2 = GetSizeOfCondition(tableDefinition, orCondition.Right);
                return sizeOfRelation * (1 - ((1 - (m1 / sizeOfRelation)) * ((m2 / sizeOfRelation))));
            }
            else if (condition is LeafCondition leafCondition)
            {
                switch (leafCondition.Operation)
                {
                    case Compiler.Common.RelOp.Equals:
                        return sizeOfRelation * (1 / GetDistinctValuesEstimate(tableDefinition, leafCondition.Column));
                    case Compiler.Common.RelOp.GreaterOrEqualThan:
                    case Compiler.Common.RelOp.GreaterThan:
                    case Compiler.Common.RelOp.LessOrEqualThan:
                    case Compiler.Common.RelOp.LessThan:
                    case Compiler.Common.RelOp.NotEquals:
                        return sizeOfRelation;
                }
            }

            return sizeOfRelation;
        }

        private int GetTotalSizeEstimate(TableDefinition tableDefinition)
        {
            PhysicalOperation operation;

            Table table = _relationManager.GetTable(tableDefinition.Id);
            if (tableDefinition.HasClusteredIndex())
            {
                operation = new IndexSeekOperation(table, table.GetIndex(tableDefinition.GetClusteredIndex().Column));
            }
            else
            {
                operation = new TableScanOperation(table);
            }

            int i = 0;
            while (operation.GetNext() != null)
            {
                i++;
            }

            return i;
        }

        private int GetDistinctValuesEstimate(TableDefinition tableDefinition, AttributeDefinition column)
        {
            throw new NotImplementedException();
        }

        public double GetSizeOfProjection(ProjectionElement projection)
        {
            double sizeOfRelation = 0;
            double sizeOfProjection = 0;

            IEnumerable<Relation> relations = projection.Columns.Select(x => x.Relation).Distinct();

            foreach (Relation relation in relations)
            {
                sizeOfRelation += GetTotalSizeOfTuple(relation, relation);
                sizeOfProjection += GetTotalSizeOfTuple(relation, projection.Columns.Where(x => x.Relation == relation).Select(x => x.AttributeDefinition).ToList());
            }

            return sizeOfProjection / sizeOfRelation;
        }

        private int GetTotalSizeOfTuple(Relation relation, List<AttributeDefinition> attributeDefinitions)
        {
            int result = 0;

            foreach (AttributeDefinition definition in attributeDefinitions)
            {
                result += GetSizeOfAttributeDefinition(relation, definition);
            }

            return result;
        }

        private int GetSizeOfAttributeDefinition(Relation relation, AttributeDefinition definition)
        {
            switch (definition.Type)
            {
                case ValueType.Boolean:
                    return 1;
                case ValueType.Integer:
                case ValueType.UnsignedInteger:
                    return 4;
                case ValueType.String:
                    // todo: max(column) statistic?
                    return 1000;
            }

            return 0;
        }
    }
}

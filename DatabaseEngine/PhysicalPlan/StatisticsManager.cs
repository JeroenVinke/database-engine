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
            CalculateStatistic(_relationManager.GetRelation(relationId) as TableDefinition, statistics);

            return statistics;
        }

        public void CalculateStatistics()
        {
            foreach (Table table in _relationManager.Tables)
            {
                TableStatistics statistics = GetStatistics(table.TableDefinition.Id);
                CalculateStatistic(table.TableDefinition, statistics);
            }
        }

        public void PrintStatistics()
        {
            Console.WriteLine("Statistics:");
            foreach (TableStatistics statistics in _statisticsPerRelation.Values)
            {
                TableDefinition table = _relationManager.GetTable(statistics.RelationId).TableDefinition;
                Console.WriteLine($"[{table.Name}]: TotalSize: {statistics.Count}");
                foreach(ColumnStatistics columnStatistic in statistics.ColumnStatistics.Values)
                {
                    Console.WriteLine($"[{table.Name}.{columnStatistic.AttributeDefinition.Name}]: DistinctValues: {columnStatistic.DistinctValuesCount}, MaxSize: {columnStatistic.MaxSize}");
                }
            }
        }

        public int T(TableDefinition tableDefinition)
        {
            return GetStatistics(tableDefinition.Id).Count;
        }

        public int B(TableDefinition tableDefinition)
        {
            double recordsPerBlock = GetBlockSize(tableDefinition) / (double)GetTotalSizeOfTuple(tableDefinition, tableDefinition);
            double ratio = 1 / recordsPerBlock;

            return (int)Math.Ceiling(GetStatistics(tableDefinition.Id).Count * ratio);
        }

        public double GetBlockSize(Relation relation)
        {
            return (double)Block.BlockSize;
        }

        private void CalculateStatistic(TableDefinition tableDefinition, TableStatistics statistics)
        {
            PhysicalOperation operation;

            Table table = _relationManager.GetTable(tableDefinition.Id);
            if (tableDefinition.HasClusteredIndex())
            {
                operation = new IndexScanOperation(new RelationElement(tableDefinition), table, table.GetIndex(tableDefinition.GetClusteredIndex().Column));
            }
            else
            {
                operation = new TableScanOperation(new RelationElement(tableDefinition), table);
            }

            operation.Prepare();

            statistics.ColumnStatistics.Clear();

            foreach (AttributeDefinition attributeDefinition in table.TableDefinition)
            {
                statistics.ColumnStatistics.Add(attributeDefinition.Name, new ColumnStatistics() { AttributeDefinition = attributeDefinition });
            }

            int i = 0;

            Dictionary<string, HashSet<object>> distinctValuesPerColumn = new Dictionary<string, HashSet<object>>();

            foreach (AttributeDefinition attributeDefinition in table.TableDefinition)
            {
                distinctValuesPerColumn.Add(attributeDefinition.Name, new HashSet<object>());
            }

            Dictionary<string, int> maxSizePerColumn = new Dictionary<string, int>();

            foreach (AttributeDefinition attributeDefinition in table.TableDefinition)
            {
                maxSizePerColumn.Add(attributeDefinition.Name, GetDefaultSize(table.TableDefinition, attributeDefinition));
            }

            CustomTuple tuple;
            do
            {
                tuple = operation.GetNext();

                if (tuple != null)
                {
                    i++;

                    foreach (AttributeDefinition attributeDefinition in table.TableDefinition)
                    {
                        object value = tuple.GetValueFor<object>(attributeDefinition.Name);
                        
                        if (!distinctValuesPerColumn[attributeDefinition.Name].Contains(value))
                        {
                            distinctValuesPerColumn[attributeDefinition.Name].Add(value);
                        }

                        if (attributeDefinition.Type == ValueType.String
                            && value is string s
                            && s.Length > maxSizePerColumn[attributeDefinition.Name])
                        {
                            maxSizePerColumn[attributeDefinition.Name] = s.Length;
                        }
                    }
                }
            }
            while (tuple != null);

            foreach (KeyValuePair<string, HashSet<object>> distinctValue in distinctValuesPerColumn)
            {
                statistics.ColumnStatistics[distinctValue.Key].DistinctValuesCount = distinctValue.Value.Count;
            }

            foreach (KeyValuePair<string, int> maxSize in maxSizePerColumn)
            {
                statistics.ColumnStatistics[maxSize.Key].MaxSize = maxSize.Value;
            }

            statistics.Count = i;
        }

        public int V(TableDefinition tableDefinition, AttributeDefinition column)
        {
            return _statisticsPerRelation[tableDefinition.Id].ColumnStatistics[column.Name].DistinctValuesCount;
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
                result += GetMaxSize(relation, definition);
            }

            return result;
        }

        private int GetDefaultSize(Relation relation, AttributeDefinition definition)
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

        private int GetMaxSize(Relation relation, AttributeDefinition definition)
        {
            return GetStatistics(relation.Id).ColumnStatistics[definition.Name].MaxSize;
        }
    }
}

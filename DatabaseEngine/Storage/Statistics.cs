using System;
using System.Collections.Generic;

namespace DatabaseEngine.Storage
{
    public class TableStatistics
    {
        public int Count { get; set; }
        public int RelationId { get; set; }
        public DateTime? LastCalculation { get; set; }
        public Dictionary<string, ColumnStatistics> ColumnStatistics { get; set; } = new Dictionary<string, ColumnStatistics>();
    }

    public class ColumnStatistics
    {
        public int DistinctValuesCount { get; set; }
        public int MaxSize { get; set; }
        public AttributeDefinition AttributeDefinition { get; set; }
    }
}

using System;

namespace DatabaseEngine.Storage
{
    public class TableStatistics
    {
        public int TotalSize { get; set; }
        public int RelationId { get; set; }
        public DateTime? LastCalculation { get; set; }
    }
}

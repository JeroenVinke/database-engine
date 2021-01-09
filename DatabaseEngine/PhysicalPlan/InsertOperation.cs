using DatabaseEngine.LogicalPlan;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class InsertOperation : PhysicalOperation
    {
        public Table Table { get; set; }

        public InsertOperation(LogicalElement logicalElement, Table table)
            : base(logicalElement)
        {
            Table = table;
        }

        public override CustomTuple GetNext()
        {
            CustomTuple tuple = Left.GetNext();

            if (tuple != null)
            {
                Table.Insert(tuple);
            }

            return tuple;
        }
    }
}

using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class InsertOperation : PhysicalOperation
    {
        public PhysicalOperation Input { get; set; }
        public Table Table { get; set; }

        public InsertOperation(Table table, PhysicalOperation input)
            : base(new List<PhysicalOperation>())
        {
            Input = input;
            Table = table;
        }

        public override CustomTuple GetNext()
        {
            CustomTuple tuple = Input.GetNext();

            if (tuple != null)
            {
                Table.Insert(tuple);
            }

            return tuple;
        }
    }
}

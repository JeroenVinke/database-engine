using DatabaseEngine.Commands;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class QueryPlan
    {
        public Command Command { get; set; }

        public QueryPlan(Command command)
        {
            Command = command;
        }

        public List<CustomTuple> Execute()
        {
            List<CustomTuple> result = new List<CustomTuple>();

            if (Command is SelectCommand selectCommand)
            {
                Operation input = new TableScanOperation(selectCommand.Table);

                if (selectCommand.Join != null)
                {
                    input = new NestedLoopJoinOperation(input, new IndexSeekOperation(selectCommand.Join.RightTable, selectCommand.Join.RightTable.GetIndex(selectCommand.Join.RightColumn)), selectCommand.Join.LeftColumn, selectCommand.Join.RightColumn);
                }

                if (selectCommand.Condition != null)
                {
                    input = new FilterOperation(input, selectCommand.Condition);
                }

                input.Prepare();

                CustomTuple next;

                do
                {
                    next = input.GetNext();
                    if (next != null)
                    {
                        result.Add(next);
                    }
                } while (next != null);

                input.Unprepare();
            }

            return result;
        }
    }
}

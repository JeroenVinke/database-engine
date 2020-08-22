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
            if (Command is SelectCommand selectCommand)
            {
                if (selectCommand.Condition != null)
                {
                    //selectCommand.Table.Indexes
                }
            }

            return new List<CustomTuple>();
        }
    }
}

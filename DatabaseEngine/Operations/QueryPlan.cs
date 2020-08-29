using DatabaseEngine.Commands;
using System.Collections.Generic;
using System.Linq;

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
                Operation input = GetFullTableScanOperation(selectCommand.Table);

                if (selectCommand.Join != null)
                {
                    Table joinTable = selectCommand.Join.RightTable;
                    input = new NestedLoopJoinOperation(input, GetFullTableScanOperation(joinTable, selectCommand.Join.RightColumn), selectCommand.Join.LeftColumn, selectCommand.Join.RightColumn);
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
            else if (Command is InsertCommand insertCommand)
            {
                insertCommand.Table.Insert(insertCommand.Values.ToArray());
            }

            return result;
        }

        public Operation GetFullTableScanOperation(Table table, AttributeDefinition column = null)
        {
            if (table.TableDefinition.HasClusteredIndex())
            {
                return new IndexSeekOperation(table, table.GetIndex(column ?? table.TableDefinition.GetClusteredIndex().Columns.First()));
            }
            else
            {
                return new TableScanOperation(table);
            }
        }
    }
}

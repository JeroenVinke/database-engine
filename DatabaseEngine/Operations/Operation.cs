using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public abstract class Operation
    {
        public Operation(List<Operation> inputOperations)
        {
            InputOperations = inputOperations;
        }

        public virtual List<Operation> InputOperations { get; } = new List<Operation>();

        public virtual void Prepare()
        {
            foreach(Operation operation in InputOperations)
            {
                operation.Prepare();
            }
        }

        public virtual CustomTuple GetNext()
        {
            return null;
        }

        public virtual void Unprepare()
        {
            foreach (Operation operation in InputOperations)
            {
                operation.Unprepare();
            }
        }
    }
}

using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public abstract class PhysicalOperation
    {
        public PhysicalOperation(List<PhysicalOperation> inputOperations)
        {
            InputOperations = inputOperations;
        }

        public virtual List<PhysicalOperation> InputOperations { get; } = new List<PhysicalOperation>();

        public virtual void Prepare()
        {
            foreach(PhysicalOperation operation in InputOperations)
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
            foreach (PhysicalOperation operation in InputOperations)
            {
                operation.Unprepare();
            }
        }

        //public abstract int EstimateNumberOfRows();
        public virtual int EstimateNumberOfRows() { return 0; }
    }
}

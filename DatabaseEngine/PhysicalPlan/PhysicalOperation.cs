using DatabaseEngine.LogicalPlan;
using System;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public abstract class PhysicalOperation
    {
        public PhysicalOperation Left { get; set; }
        public PhysicalOperation Right { get; set; }

        public PhysicalOperation(LogicalElement logicalElement)
        {
            LogicalElement = logicalElement;
        }

        public void SetInput(PhysicalOperation left, PhysicalOperation right)
        {
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        public LogicalElement LogicalElement { get; }

        public virtual int EstimateIOCost() => 0;
        public virtual int EstimateCPUCost() => 0;
        public virtual int GetCost() => EstimateCPUCost() + (int)Math.Round(Math.Pow(EstimateIOCost(), 4));

        public virtual void Prepare()
        {
            Left?.Prepare();
            Right?.Prepare();
        }

        public virtual CustomTuple GetNext()
        {
            return null;
        }

        public virtual void Unprepare()
        {
            Left?.Unprepare();
            Right?.Unprepare();
        }
    }
}

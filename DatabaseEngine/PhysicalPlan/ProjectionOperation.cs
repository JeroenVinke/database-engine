using DatabaseEngine.LogicalPlan;
using System;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class ProjectionOperation : PhysicalOperation
    {
        private List<AttributeDefinition> _projectionColumns;

        public ProjectionOperation(LogicalElement logicalElement, PhysicalOperation inputOperation, List<AttributeDefinition> projectionColumns)
            :base (logicalElement)
        {
            Left = inputOperation;
            _projectionColumns = projectionColumns;
        }

        public override CustomTuple GetNext()
        {
            CustomTuple tuple = Left.GetNext();

            if (tuple != null)
            {
                return tuple.Projection(_projectionColumns);
            }

            return null;
        }
    }
}

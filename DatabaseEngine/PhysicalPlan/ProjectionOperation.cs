using System;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class ProjectionOperation : PhysicalOperation
    {
        private PhysicalOperation _inputOperation;
        private List<AttributeDefinition> _projectionColumns;

        public ProjectionOperation(PhysicalOperation inputOperation, List<AttributeDefinition> projectionColumns)
            :base (new List<PhysicalOperation> { inputOperation })
        {
            _inputOperation = inputOperation;
            _projectionColumns = projectionColumns;
        }

        public override CustomTuple GetNext()
        {
            CustomTuple tuple = _inputOperation.GetNext();

            if (tuple != null)
            {
                return tuple.Projection(_projectionColumns);
            }

            return null;
        }
    }
}

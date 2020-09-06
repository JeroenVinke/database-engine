using System;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class ProjectionOperation : Operation
    {
        private Operation _inputOperation;
        private List<AttributeDefinition> _projectionColumns;

        public ProjectionOperation(Operation inputOperation, List<AttributeDefinition> projectionColumns)
            :base (new List<Operation> { inputOperation })
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

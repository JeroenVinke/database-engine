using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine.Operations
{
    public class MemorySetOperation : PhysicalOperation
    {
        public List<CustomTuple> Set { get; set; }
        private int _currentIndex = 0;

        // probably should not have data in the queryplan itself
        public MemorySetOperation(Set set)
            : base(new List<PhysicalOperation>())
        {
            Set = set.ToList();
        }

        public override void Prepare()
        {
            base.Prepare();
            _currentIndex = 0;
        }

        public override CustomTuple GetNext()
        {
            if (Set.Count > _currentIndex)
            {
                CustomTuple tuple = Set[_currentIndex];
                _currentIndex++;

                return tuple;
            }

            return null;
        }
    }
}

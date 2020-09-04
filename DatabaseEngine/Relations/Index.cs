using DatabaseEngine.Models;
using System.Collections.Generic;

namespace DatabaseEngine
{
    public class Index
    {
        [FromColumn("Column")]
        public string Column { get; set; }

        [FromColumn("IsClustered")]
        public bool IsClustered { get; set; }
        [FromColumn("RootBlockId")]
        public uint RootBlockId { get; set; }
        public Pointer RootPointer
        {
            get
            {
                return new Pointer(RootBlockId);
            }
            set
            {
                RootBlockId = value.Short;
            }
        }
    }
}

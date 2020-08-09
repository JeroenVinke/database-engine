using System.Diagnostics;

namespace DatabaseEngine
{
    [DebuggerDisplay("BPlusTreeNodeValue: {Value}")]
    public class BPlusTreeNodeValue
    {
        public int Value { get; set; }
        public Pointer Pointer { get; set; }
        public Pointer LeftPointer { get; set; }
        public Pointer RightPointer { get; set; }
    }
}

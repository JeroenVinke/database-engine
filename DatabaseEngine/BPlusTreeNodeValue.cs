using System.Diagnostics;

namespace DatabaseEngine
{
    [DebuggerDisplay("BPlusTreeNodeValue: {Value}")]
    public class BPlusTreeNodeValue
    {
        public int Value { get; set; }
        public BPlusTreeNode Pointer { get; set; }
    }
}

using System.Collections.Generic;

namespace DatabaseEngine
{
    public interface IBPlusTreeNode
    {
        bool IsRoot { get; set; }
        bool IsLeaf { get; set; }
        int Id { get; set; }
        void ReadNode();
        IBPlusTreeNode AddValue(object value, Pointer valuePointer);
        Pointer Find(object value, bool exact = true);
        List<BPlusTreeNodeValue> Values { get; set; }
        void WriteTree();
        void WriteNode();
        IBPlusTreeNode GetFirstLeaf();
        (IBPlusTreeNode, int) FindFirstNodeForCondition(LeafCondition condition);
        (IBPlusTreeNode, int) FindLastNodeForCondition(LeafCondition condition);
        IBPlusTreeNode LeftSibling { get; }
        IBPlusTreeNode RightSibling { get; }
    }
}
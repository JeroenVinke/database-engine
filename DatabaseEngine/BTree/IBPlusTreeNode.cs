namespace DatabaseEngine
{
    public interface IBPlusTreeNode
    {
        bool IsRoot { get; set; }
        bool IsLeaf { get; set; }
        void ReadNode();
        IBPlusTreeNode AddValue(object value, Pointer valuePointer);
        Pointer Find(object value, bool exact = true);
        void WriteTree();
        void WriteNode();
    }
}
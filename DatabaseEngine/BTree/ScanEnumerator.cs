using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine.BTree
{
    public class ScanEnumerator : IEnumerator, IEnumerator<Pointer>
    {
        public Relation Relation { get; set; }
        public IBPlusTreeNode Index { get; set; }
        public LeafCondition Condition { get; set; }
        public Block CurrentBlock { get; set; }
        public Pointer Last { get; set; }
        public IBPlusTreeNode CurrentNode { get; set; }
        public BPlusTreeNodeValue CurrentValue { get; set; }

        public object Current => CurrentValue.Pointer;
        Pointer IEnumerator<Pointer>.Current => CurrentValue.Pointer;
        private int _i;

        public ScanEnumerator(Relation relation, IBPlusTreeNode index, Condition condition)
        {
            Relation = relation;
            Index = index;
            Condition = condition as LeafCondition;

            Reset();
        }

        public bool MoveNext()
        {
            CurrentValue = CurrentNode?.Values.Count > _i ? CurrentNode.Values[_i] : null;

            if (CurrentValue == null)
            {
                if (CurrentNode?.Sibling != null)
                {
                    CurrentNode = CurrentNode.Sibling;
                    _i = 0;
                    CurrentValue = CurrentNode.Values.Count >= _i ? CurrentNode.Values[_i] : null;
                }
            }

            _i++;

            return CurrentValue != null;
        }

        public void Reset()
        {
            if (CurrentNode == null)
            {
                CurrentNode = Index.FindFirstNodeForCondition(Condition);
            }
            _i = 0;
        }

        public void Dispose()
        {
            Last = null;
            CurrentBlock = null;
        }
    }
}

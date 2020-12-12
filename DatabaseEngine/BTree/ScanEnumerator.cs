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
        public bool LeftToRight { get; }
        public Block CurrentBlock { get; set; }
        public Pointer Last { get; set; }
        public IBPlusTreeNode CurrentNode { get; set; }
        public BPlusTreeNodeValue CurrentValue { get; set; }

        public object Current => CurrentValue.Pointer;
        Pointer IEnumerator<Pointer>.Current => CurrentValue.Pointer;
        private int _i;

        public ScanEnumerator(Relation relation, IBPlusTreeNode index, Condition condition, bool leftToRight)
        {
            Relation = relation;
            Index = index;
            Condition = condition as LeafCondition;
            LeftToRight = leftToRight;

            Reset();
        }

        public bool MoveNext()
        {
            if (LeftToRight)
            {
                CurrentValue = CurrentNode?.Values.Count > _i ? CurrentNode.Values[_i] : null;
            }
            else
            {
                CurrentValue = _i >= 0 ? CurrentNode.Values[_i] : null;
            }

            if (CurrentValue == null)
            {
                if ((LeftToRight && CurrentNode?.RightSibling != null) || (!LeftToRight && CurrentNode?.LeftSibling != null))
                {
                    CurrentNode = LeftToRight ? CurrentNode.RightSibling : CurrentNode.LeftSibling;

                    if (LeftToRight)
                    {
                        _i = 0;
                    }
                    else
                    {
                        _i = CurrentNode.Values.Count - 1;
                    }

                    CurrentValue = CurrentNode.Values.Count >= _i ? CurrentNode.Values[_i] : null;
                }
            }

            if (LeftToRight)
            {
                _i++;
            }
            else
            {
                _i--;
            }

            if (CurrentValue != null && Condition != null)
            {
                CustomTuple tuple = new CustomTuple(Relation);
                tuple.AddValueFor(Condition.Column.Name, (int)CurrentValue.Value);

                if (!Condition.SatisfiesCondition(tuple))
                {
                    return MoveNext();
                }
            }

            return CurrentValue != null;
        }

        public void Reset()
        {
            _i = 0;
            if (CurrentNode == null)
            {
                if (LeftToRight)
                {
                    (CurrentNode, _i) = Index.FindFirstNodeForCondition(Condition);
                }
                else
                {
                    (CurrentNode, _i) = Index.FindLastNodeForCondition(Condition);
                }
            }
        }

        public void Dispose()
        {
            Last = null;
            CurrentBlock = null;
        }
    }
}

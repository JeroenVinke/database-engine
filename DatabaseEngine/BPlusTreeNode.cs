using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace DatabaseEngine
{
    [DebuggerDisplay("Values: {Values}, Min: {Min}, Max: {Max}, IsRoot: {IsRoot}, Children: {Children}")]
    public class BPlusTreeNode
    {
        // Pointers
        public List<BPlusTreeNode> Children { get; set; } = new List<BPlusTreeNode>();

        public Dictionary<long, BPlusTreeNode> NodeCache { get; set; } = new Dictionary<long, BPlusTreeNode>();

        public bool IsLeaf => Children.Count == 0;
        public int MaxSize { get; set; } = 3;
        public bool IsRoot => Parent == null;

        public List<BPlusTreeNodeValue> Values { get; set; } = new List<BPlusTreeNodeValue>();
        public int Min => Values.Select(x => x.Value).Min();
        public static int MaxId = 0;
        public int Id { get; set; }

        public BPlusTreeNode Sibling { get; set; }
        public BPlusTreeNode Parent { get; set; }
        public StorageFile StorageFile { get; set; } 
        public Pointer Pointer { get; set; }

        public BPlusTreeNode(Pointer pointer)
        {
            Id = MaxId++;
            Pointer = pointer;
        }

        public BPlusTreeNode ReadNode(long pointerValue)
        {
            if (NodeCache.ContainsKey(pointerValue))
            {
                return NodeCache[pointerValue];
            }

            Pointer pointer = Pointer.GetPointerFromLong(pointerValue);

            Block block = StorageFile.ReadBlock(pointer.PageNumber);

            return new BPlusTreeNode(pointer);
        }

        private BPlusTreeNode ReadNode(Pointer pointer)
        {
            return ReadNode(pointer.GetPointerAsLong());
        }

        public BPlusTreeNode AddValue(int value)
        {
            if (IsLeaf)
            {
                AddValueToSelf(new BPlusTreeNodeValue { Pointer = null, Value = value });
            }
            else
            {
                AddValueToChild(value);
            }

            return this;
        }

        private void Split()
        {
            if (IsRoot)
            {
                int half = (int)Math.Ceiling(Values.Count / (double)2);
                List<BPlusTreeNodeValue> firstHalf = Values.Take(half).ToList();
                List<BPlusTreeNodeValue> secondHalf = Values.Skip(half).ToList();

                Children.Clear();

                BPlusTreeNode leftChild = AddChild();
                leftChild.Values = firstHalf;
                foreach (BPlusTreeNodeValue value in firstHalf)
                {
                    if (value.Pointer != null)
                    {
                        BPlusTreeNode node = ReadNode(value.Pointer);
                        node.Parent = leftChild;
                        leftChild.Children.Add(node);
                    }
                }

                BPlusTreeNode rightChild = AddChild();
                rightChild.Values = secondHalf;
                foreach (BPlusTreeNodeValue value in secondHalf)
                {
                    if (value.Pointer != null)
                    {
                        BPlusTreeNode node = ReadNode(value.Pointer);
                        node.Parent = rightChild;
                        rightChild.Children.Add(node);
                    }
                }

                BPlusTreeNodeValue middle = new BPlusTreeNodeValue { Pointer = rightChild.Pointer, Value = Values[half].Value };
                Values = new List<BPlusTreeNodeValue> { middle };

                if (leftChild.IsLeaf)
                {
                    leftChild.Sibling = rightChild;
                }
            }
            else
            {
                int half = Values.Count / 2;
                List<BPlusTreeNodeValue> firstHalf = Values.Take(half).ToList();
                List<BPlusTreeNodeValue> secondHalf = Values.Skip(half).ToList();

                BPlusTreeNode rightChild = Parent.AddChild();
                rightChild.Values = secondHalf;

                Values = firstHalf;

                if (IsLeaf)
                {
                    Sibling = rightChild;
                }

                Parent.AddValueToSelf(new BPlusTreeNodeValue { Value = secondHalf.First().Value, Pointer = rightChild.Pointer });
            }
        }

        public void AddValueToSelf(BPlusTreeNodeValue value)
        {
            int target = -1;

            for (int i = 0; i < Values.Count; i++)
            {
                if (value.Value < Values[i].Value)
                {
                    target = i;
                    break;
                }
            }

            if (target == -1)
            {
                Values.Add(value);
            }
            else
            {
                Values.Insert(target, value);
            }

            if (Values.Count > MaxSize)
            {
                Split();
            }
        }

        public void AddValueToChild(int value)
        {
            BPlusTreeNode target = null;

            for (int i = Children.Count - 1; i >= 0; i--)
            {
                if (value > Children[i].Min)
                {
                    target = Children[i];
                    break;
                }
            }

            if (target != null)
            {
                target.AddValue(value);
            }
            else
            {
                Children.Last().AddValue(value);
            }
        }

        public BPlusTreeNode AddChild()
        {
            BPlusTreeNode child = new BPlusTreeNode(StorageFile.GetFreeBlock());
            child.MaxSize = child.MaxSize;
            child.Parent = this;
            child.NodeCache = NodeCache;
            child.StorageFile = StorageFile;
            Children.Add(child);
            return child;
        }

        public string ToDot()
        {
            string dot = $"\rgroup_" + Id + " ["
                + "\rshape = plaintext"
                + "\rlabel =<"
                + "\r<table border = \"1\" cellborder = '1' cellspacing = \"0\" cellpadding = \"0\"><tr>";

            for (int i = 0; i < Values.Count; i++)
            {
                dot += "<td>" + Values[i].Value + "</td>";
            }


            dot += "</tr></table>";
            dot += ">]";

            if (Children.Count > 0)
            {
                string rank = "{rank=same ";
                foreach (BPlusTreeNode node in Children)
                {
                    dot += node.ToDot();
                    rank += " group_" + node.Id;
                    dot += "\rgroup_" + Id + " -> group_" + node.Id;
                }
                rank += "}";
                dot += "\r" + rank;
            }

            if (Sibling != null)
            {
                dot += "\rgroup_" + Id + " -> group_" + Sibling.Id;
            }

            return dot;
        }
    }
}

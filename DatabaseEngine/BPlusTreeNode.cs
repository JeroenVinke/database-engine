using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DatabaseEngine
{
    [DebuggerDisplay("Values: {Values}, Min: {Min}, Max: {Max}, IsRoot: {IsRoot}, IsLeaf: {IsLeaf}, Pointers: {Pointers}, Values: {Values}")]
    public class BPlusTreeNode
    {
        public Dictionary<long, BPlusTreeNode> NodeCache { get; set; } = new Dictionary<long, BPlusTreeNode>();

        public bool IsLeaf { get; set; }
        public int MaxSize { get; set; } = 3;
        public bool IsRoot => Parent == null;

        public List<Pointer> Pointers { get; set; } = new List<Pointer>();
        public List<BPlusTreeNodeValue> Values { get; set; } = new List<BPlusTreeNodeValue>();
        public IEnumerable<BPlusTreeNode> PointerNodes => Pointers.Select(x => ReadNode(x));
        public int Min => Values.Select(x => x.Value).Min();
        public int Max => Values.Select(x => x.Value).Max();
        public static int MaxId = 0;
        public int Id { get; set; }

        public BPlusTreeNode Sibling { get; set; }
        public BPlusTreeNode Parent { get; set; }
        public StorageFile StorageFile { get; set; } 
        public Pointer Page { get; set; }

        public BPlusTreeNode(Pointer page)
        {
            Id = MaxId++;
            Page = page;
        }

        public BPlusTreeNode ReadNode(long pointerValue)
        {
            if (NodeCache.ContainsKey(pointerValue))
            {
                return NodeCache[pointerValue];
            }

            Pointer pointer = Pointer.GetPointerFromLong(pointerValue);

            Block block = StorageFile.ReadBlock(pointer.PageNumber);

            if (block.Header.Type == BlockType.Data)
            {
                return null;
            }

            BPlusTreeNode node = new BPlusTreeNode(pointer); // pointer

            NodeCache.Add(pointerValue, node);

            return node;
        }

        public Pointer Find(int value)
        {
            if (IsLeaf)
            {
                return Pointers[Values.FindIndex(x => x.Value == value)];
            }
            else
            {
                Pointer target = GetTargetPointer(value);
                BPlusTreeNode node = ReadNode(target);
                return node.Find(value);
            }
        }

        private BPlusTreeNode ReadNode(Pointer pointer)
        {
            return ReadNode(pointer.GetPointerAsLong());
        }

        public BPlusTreeNode AddValue(int value, Pointer valuePointer)
        {
            if (IsLeaf)
            {
                AddValueToSelf(new BPlusTreeNodeValue { Pointer = valuePointer, Value = value });
            }
            else
            {
                AddValueToChild(value, valuePointer);
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

                BPlusTreeNode leftChild = AddChild();
                leftChild.Values = firstHalf;
                leftChild.IsLeaf = IsLeaf;
                foreach (BPlusTreeNodeValue value in firstHalf)
                {
                    if (value.Pointer != null)
                    {
                        BPlusTreeNode node = ReadNode(value.Pointer);

                        if (node != null)
                        {
                            node.Parent = leftChild;
                        }
                        leftChild.AddPointer(value.Pointer);
                    }
                }

                BPlusTreeNode rightChild = AddChild();
                rightChild.IsLeaf = IsLeaf;
                rightChild.Values = secondHalf;
                foreach (BPlusTreeNodeValue value in secondHalf)
                {
                    if (value.Pointer != null)
                    {
                        BPlusTreeNode node = ReadNode(value.Pointer);
                        if (node != null)
                        {
                            node.Parent = rightChild;
                        }
                        rightChild.AddPointer(value.Pointer);
                    }
                }

                BPlusTreeNodeValue middle = new BPlusTreeNodeValue { Value = Values[half].Value, Pointer = rightChild.Page };
                Values = new List<BPlusTreeNodeValue> { middle };
                SetPointers(new List<Pointer> { leftChild.Page, rightChild.Page });

                if (leftChild.IsLeaf)
                {
                    leftChild.Sibling = rightChild;
                }

                IsLeaf = false;
            }
            else
            {
                int half = Values.Count / 2;
                List<BPlusTreeNodeValue> firstHalf = Values.Take(half).ToList();
                List<BPlusTreeNodeValue> secondHalf = Values.Skip(half).ToList();

                BPlusTreeNode rightChild = Parent.AddChild();
                rightChild.Values = secondHalf;
                rightChild.SetPointers(Pointers.Skip(half).ToList());

                Values = firstHalf;
                SetPointers(Pointers.Take(half).ToList());

                if (IsLeaf)
                {
                    Sibling = rightChild;
                    rightChild.IsLeaf = true;
                }

                Parent.AddValueToSelf(new BPlusTreeNodeValue { Value = secondHalf.First().Value, Pointer = rightChild.Page });
            }
        }

        private void AddPointer(Pointer pointer, int index = -1)
        {
            if (Pointers.Any(x => x.GetPointerAsLong() == pointer.GetPointerAsLong()))
            {
                return;
            }

            if (index == -1)
            {
                Pointers.Add(pointer);
            }
            else
            {
                Pointers.Insert(index, pointer);
            }
        }

        private void SetPointers(IEnumerable<Pointer> pointers)
        {
            Pointers = new List<Pointer>();
            foreach(Pointer pointer in pointers)
            {
                AddPointer(pointer);
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
                AddPointer(value.Pointer);
            }
            else
            {
                Values.Insert(target, value);
                AddPointer(value.Pointer, target);
            }

            if (Values.Count > MaxSize)
            {
                Split();
            }
        }

        public void AddValueToChild(int value, Pointer valuePointer)
        {
            BPlusTreeNode target = ReadNode(GetTargetPointer(value));

            target.AddValue(value, valuePointer);
        }

        private Pointer GetTargetPointer(int value)
        {
            for (int i = Values.Count - 1; i >= 0; i--)
            {
                if (value > Values[i].Value)
                {
                    return Pointers[i+1];
                }
            }

            return Pointers[0];
        }

        public BPlusTreeNode AddChild()
        {
            BPlusTreeNode child = new BPlusTreeNode(StorageFile.GetFreeBlock());
            child.MaxSize = child.MaxSize;
            child.Parent = this;
            child.NodeCache = NodeCache;
            child.StorageFile = StorageFile;
            //AddPointer(child.Page);
            NodeCache.Add(child.Page.GetPointerAsLong(), child);
            return child;
        }

        public string ToDot()
        {
            string dot = "";

            if (Values.Count > 0)
            {
                dot = $"\rgroup_" + Id + " ["
                   + "\rshape = plaintext"
                   + "\rlabel =<"
                   + "\r<table border = \"1\" cellborder = '1' cellspacing = \"0\" cellpadding = \"0\"><tr>";

                for (int i = 0; i < Values.Count; i++)
                {
                    dot += "<td>" + Values[i].Value + "</td>";
                }

                dot += "</tr></table>";
                dot += ">]";
            }
            else
            {
                dot = $"\rgroup_" + Id + " ["
                   + "\rshape = plaintext"
                   + "\rlabel = <No values>]";
            }

            if (Pointers.Count() > 0)
            {
                string rank = "";
                foreach (Pointer pointer in Pointers)
                {
                    BPlusTreeNode node = ReadNode(pointer);

                    if (node != null)
                    {
                        dot += node.ToDot();
                        rank += " group_" + node.Id;
                        dot += "\rgroup_" + Id + " -> group_" + node.Id;
                    }
                    else
                    {
                        string group = "data_" + pointer.PageNumber + "_" + pointer.Index;
                        dot += "\r" + group;
                        dot += "\rgroup_" + Id + " -> " + group;
                    }
                }

                if (!string.IsNullOrEmpty(rank))
                {
                    rank = "{rank=same " + rank + "}";
                    dot += "\r" + rank;
                }
            }

            if (Sibling != null)
            {
                dot += "\rgroup_" + Id + " -> group_" + Sibling.Id;
            }

            return dot;
        }
    }
}

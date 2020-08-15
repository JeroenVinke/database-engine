using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DatabaseEngine
{
    [DebuggerDisplay("Values: {Values}, Min: {Min}, Max: {Max}, IsRoot: {IsRoot}, IsLeaf: {IsLeaf}, Pointers: {Pointers}, Values: {Values}")]
    public class BPlusTreeNode
    {
        public NodeCache NodeCache { get; set; }

        public bool IsLeaf { get; set; }
        public int MaxSize { get; set; } = 3;
        public bool IsRoot { get; set; }

        public List<BPlusTreeNodeValue> Values { get; set; } = new List<BPlusTreeNodeValue>();
        public IEnumerable<BPlusTreeNode> PointerNodes => Values.SelectMany(x => new List<Pointer> { x.LeftPointer, x.RightPointer }).Where(x => x != null).Select(x => ReadNode(x));

        public int Min => Values.Select(x => x.Value).Min();
        public int Max => Values.Select(x => x.Value).Max();
        public static int MaxId = 0;
        public int Id { get; set; }

        public BPlusTreeNode Sibling { get; set; }
        public BPlusTreeNode Parent { get; set; }
        public bool Dirty { get; set; }
        public StorageFile StorageFile { get; set; }
        public Pointer Page { get; set; }
        public Block Block { get; set; }

        public BPlusTreeNode(StorageFile storageFile, Pointer page, NodeCache cache = null)
        {
            Id = MaxId++;
            Page = page;
            StorageFile = storageFile;
            NodeCache = cache ?? new NodeCache();
            NodeCache.Add(page.Short, this);
        }


        public BPlusTreeNode ReadNode(short pointerValue)
        {
            if (NodeCache.ContainsKey(pointerValue))
            {
                return NodeCache[pointerValue];
            }

            Pointer pointer = new Pointer(pointerValue);

            Block block = StorageFile.ReadBlock(pointer.PageNumber);

            if (block.Header.Type == BlockType.Data)
            {
                return null;
            }

            return GetNodeFromBlock(pointer, block);
        }

        public void ReadNode()
        {
            Block block = StorageFile.ReadBlock(Page.PageNumber);

            if (block.Header.Type == BlockType.Data)
            {
                return;
            }

            if (block.Header.Type == BlockType.Free)
            {
                IsLeaf = true;
                return;
            }

            FillNodeFromBlock(this, block);
        }

        internal void Write()
        {
            if (Block == null)
            {
                Block = new IndexBlock();
            }

            foreach (BPlusTreeNodeValue value in Values)
            {
                Block.AddRecord(new IndexRecord
                {
                    Value = value.Value,
                    LeftPointer = value.LeftPointer,
                    Pointer = value.Pointer,
                    RightPointer = value.RightPointer
                });
            }

            StorageFile.WriteBlock(Page.PageNumber, Block.ToBytes());
        }

        private BPlusTreeNode GetNodeFromBlock(Pointer pointer, Block block)
        {
            BPlusTreeNode node = new BPlusTreeNode(StorageFile, pointer, NodeCache); // pointer
            node.NodeCache = NodeCache;
            node.Parent = this;

            FillNodeFromBlock(node, block);

            return node;
        }

        internal Pointer GetPointerFor(int value)
        {
            if (IsLeaf)
            {
                return Page;
            }
            else
            {
                Pointer target = GetTargetPointer(value);
                BPlusTreeNode node = ReadNode(target);
                return node.GetPointerFor(value);
            }
        }

        private void FillNodeFromBlock(BPlusTreeNode node, Block block)
        {
            foreach (IndexRecord record in block.Records)
            {
                node.Values.Add(new BPlusTreeNodeValue
                {
                    Value = record.Value,
                    LeftPointer = record.LeftPointer,
                    Pointer = record.Pointer,
                    RightPointer = record.RightPointer
                });

                if (!IsLeaf && record.LeftPointer == null && record.RightPointer == null)
                {
                    node.IsLeaf = true;
                }
            }

            if (block.Records.Count == 0)
            {
                node.IsLeaf = true;
            }

            node.Block = block;
        }

        public IEnumerable<BPlusTreeNode> GetDirty()
        {
            return NodeCache.Where(x => x.Value.Dirty).Select(x => x.Value);
        }

        public Pointer Find(int value, bool exact = true)
        {
            if (IsLeaf)
            {
                for(int i = Values.Count - 1; i >= 0; i--)
                {
                    if (exact && value == Values[i].Value)
                    {
                        return Values[i].Pointer;
                    }
                    
                    if (!exact && value > Values[i].Value)
                    {
                        return new Pointer(Values[i].Pointer.PageNumber, 0);
                    }
                }

                if (exact)
                {
                    throw new Exception("Expected to find value");
                }
                else if (Values.Count > 0)
                {
                    return new Pointer(Values[0].Pointer.PageNumber, 0);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Pointer target = GetTargetPointer(value);
                BPlusTreeNode node = ReadNode(target);
                return node.Find(value, exact);
            }
        }

        private BPlusTreeNode ReadNode(Pointer pointer)
        {
            return ReadNode(pointer.Short);
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
                //foreach (BPlusTreeNodeValue value in firstHalf)
                //{
                //    BPlusTreeNode node = ReadNode(value.LeftPointer);

                //    if (node != null)
                //    {
                //        node.Parent = leftChild;
                //    }

                //    node = ReadNode(value.RightPointer);

                //    if (node != null)
                //    {
                //        node.Parent = leftChild;
                //    }
                //}

                BPlusTreeNode rightChild = AddChild();
                rightChild.IsLeaf = IsLeaf;
                rightChild.Values = secondHalf;
                //foreach (BPlusTreeNodeValue value in secondHalf)
                //{
                //    if (value.Pointer != null)
                //    {
                //        BPlusTreeNode node = ReadNode(value.Pointer);
                //        if (node != null)
                //        {
                //            node.Parent = rightChild;
                //        }
                //    }
                //}

                BPlusTreeNodeValue middle = new BPlusTreeNodeValue { Value = Values[half].Value, LeftPointer = leftChild.Page, RightPointer = rightChild.Page };
                Values = new List<BPlusTreeNodeValue> { middle };

                if (leftChild.IsLeaf)
                {
                    leftChild.Sibling = rightChild;
                }

                IsLeaf = false;
                Dirty = true;
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
                    rightChild.IsLeaf = true;
                }

                Parent.AddValueToSelf(new BPlusTreeNodeValue { Value = secondHalf.First().Value, LeftPointer = null, RightPointer = rightChild.Page });

                Dirty = true;
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

            Dirty = true;

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
                if (value >= Values[i].Value)
                {
                    return Values[i].RightPointer;
                }
            }

            return Values[0].LeftPointer;
        }

        public BPlusTreeNode AddChild()
        {
            BPlusTreeNode child = new BPlusTreeNode(StorageFile, StorageFile.GetFreeBlock(), NodeCache);
            child.MaxSize = child.MaxSize;
            child.Parent = this;
            child.Dirty = true;
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

                string rank = "";

                for (int i = 0; i < Values.Count; i++)
                {
                    if (i == 0 && Values[i].LeftPointer != null)
                    {
                        BPlusTreeNode node = ReadNode(Values[i].LeftPointer);

                        dot += node.ToDot();
                        rank += " group_" + node.Id;
                        dot += "\rgroup_" + Id + " -> group_" + node.Id;
                    }

                    if (Values[i].Pointer != null)
                    {
                        string group = "data_" + Values[i].Pointer.PageNumber + "_" + Values[i].Pointer.Index;
                        dot += "\r" + group;
                        dot += "\rgroup_" + Id + " -> " + group;
                    }

                    if (Values[i].RightPointer != null)
                    {
                        BPlusTreeNode node = ReadNode(Values[i].RightPointer);

                        dot += node.ToDot();
                        rank += " group_" + node.Id;
                        dot += "\rgroup_" + Id + " -> group_" + node.Id;
                    }
                }

                if (!string.IsNullOrEmpty(rank))
                {
                    rank = "{rank=same " + rank + "}";
                    dot += "\r" + rank;
                }
            }
            else
            {
                dot = $"\rgroup_" + Id + " ["
                   + "\rshape = plaintext"
                   + "\rlabel = <No values>]";
            }

            if (Sibling != null)
            {
                dot += "\rgroup_" + Id + " -> group_" + Sibling.Id;
            }

            return dot;
        }
    }
}

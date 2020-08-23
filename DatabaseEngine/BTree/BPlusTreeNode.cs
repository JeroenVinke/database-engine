using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DatabaseEngine
{
    [DebuggerDisplay("Values: {Values}, IsRoot: {IsRoot}, IsLeaf: {IsLeaf}, Pointers: {Pointers}, Values: {Values}")]
    public class BPlusTreeNode<TKeyType> : IBPlusTreeNode where TKeyType : IComparable
    {
        public NodeCache<TKeyType> NodeCache { get; set; }

        public List<BPlusTreeNodeValue<TKeyType>> Values { get; set; } = new List<BPlusTreeNodeValue<TKeyType>>();
        public IEnumerable<BPlusTreeNode<TKeyType>> PointerNodes => Values.SelectMany(x => new List<Pointer> { x.LeftPointer, x.RightPointer }).Where(x => x != null).Select(x => ReadNode(x));

        public static int MaxId = 0;
        public int Id { get; set; }

        public BPlusTreeNode<TKeyType> Sibling { get; set; }
        public BPlusTreeNode<TKeyType> Parent { get; set; }
        public bool Dirty { get; set; }
        public StorageFile StorageFile { get; set; }
        public Pointer Page { get; set; }
        public Block Block { get; set; }
        public bool IsLeaf { get; set; }
        public int MaxSize { get; set; } = 3;
        public bool IsRoot { get; set; }
        public Relation Relation { get; set; }

        public BPlusTreeNode(Relation relation, StorageFile storageFile, Pointer page, NodeCache<TKeyType> cache = null)
        {
            Id = MaxId++;
            Page = page;
            Relation = relation;
            StorageFile = storageFile;
            NodeCache = cache ?? new NodeCache<TKeyType>();
            NodeCache.Add(page.Short, this);
        }


        private BPlusTreeNode<TKeyType> ReadNode(int pointerValue)
        {
            if (NodeCache.ContainsKey(pointerValue))
            {
                return NodeCache[pointerValue];
            }

            Pointer pointer = new Pointer(pointerValue);

            Block block = StorageFile.ReadBlock(Relation, pointer);

            return GetNodeFromBlock(pointer, block);
        }

        public void ReadNode()
        {
            Block block = StorageFile.ReadBlock(Relation, Page);

            if (block.Header.Empty)
            {
                IsLeaf = true;
                return;
            }

            FillNodeFromBlock(this, block);
        }

        public void WriteTree()
        {
            IEnumerable<IBPlusTreeNode> dirtyNodes = GetDirty();

            foreach (IBPlusTreeNode dirtyNode in dirtyNodes)
            {
                dirtyNode.WriteNode();
            }
        }

        public void WriteNode()
        {
            if (Block == null)
            {
                Block = new Block(Relation);
            }

            foreach (BPlusTreeNodeValue<TKeyType> value in Values)
            {
                CustomTuple tuple = new CustomTuple(Relation);
                tuple.AddValueFor("Value", value.Value);

                if (value.LeftPointer != null)
                {
                    tuple.AddValueFor("LeftPointer", value.LeftPointer.Short);
                }
                else
                {
                    tuple.AddValueFor("LeftPointer", -1);
                }

                if (value.Pointer != null)
                {
                    tuple.AddValueFor("ValuePointer", value.Pointer.Short);
                }
                else
                {
                    tuple.AddValueFor("ValuePointer", -1);
                }

                if (value.RightPointer != null)
                {
                    tuple.AddValueFor("RightPointer", value.RightPointer.Short);
                }
                else
                {
                    tuple.AddValueFor("RightPointer", -1);
                }

                Block.AddRecord(tuple.ToRecord());
            }

            StorageFile.WriteBlock(Page.PageNumber, Block.ToBytes());
        }

        private BPlusTreeNode<TKeyType> GetNodeFromBlock(Pointer pointer, Block block)
        {
            BPlusTreeNode<TKeyType> node = new BPlusTreeNode<TKeyType>(Relation, StorageFile, pointer, NodeCache); // pointer
            node.NodeCache = NodeCache;
            node.Parent = this;

            FillNodeFromBlock(node, block);

            return node;
        }

        internal Pointer GetPointerFor(TKeyType value)
        {
            if (IsLeaf)
            {
                return Page;
            }
            else
            {
                Pointer target = GetTargetPointer(value);
                BPlusTreeNode<TKeyType> node = ReadNode(target);
                return node.GetPointerFor(value);
            }
        }

        private void FillNodeFromBlock(BPlusTreeNode<TKeyType> node, Block block)
        {
            foreach (Record record in block.Records)
            {
                CustomTuple tuple = new CustomTuple(Relation).FromRecord(record);

                int leftPointer = tuple.GetValueFor<int>("LeftPointer");
                int valuePointer = tuple.GetValueFor<int>("ValuePointer");
                int rightPointer = tuple.GetValueFor<int>("RightPointer");

                BPlusTreeNodeValue<TKeyType> value = new BPlusTreeNodeValue<TKeyType>
                {
                    Value = tuple.GetValueFor<TKeyType>("Value")
                };

                if (leftPointer >= 0)
                {
                    value.LeftPointer = new Pointer(leftPointer);
                }

                if (valuePointer >= 0)
                {
                    value.Pointer = new Pointer(valuePointer);
                }

                if (rightPointer >= 0)
                {
                    value.RightPointer = new Pointer(rightPointer);
                }

                node.Values.Add(value);

                if (!IsLeaf && leftPointer == -1 && rightPointer == -1)
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

        public IEnumerable<BPlusTreeNode<TKeyType>> GetDirty()
        {
            return NodeCache.Where(x => x.Value.Dirty).Select(x => x.Value);
        }

        public Pointer Find(object value, bool exact = true)
        {
            if (IsLeaf)
            {
                for(int i = Values.Count - 1; i >= 0; i--)
                {
                    if (exact && Compare(value, Values[i].Value) == 0)
                    {
                        return Values[i].Pointer;
                    }

                    if (!exact && Compare(value, Values[i].Value) > 0)
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
                Pointer target = GetTargetPointer((TKeyType)value);
                BPlusTreeNode<TKeyType> node = ReadNode(target);
                return node.Find(value, exact);
            }
        }

        private BPlusTreeNode<TKeyType> ReadNode(Pointer pointer)
        {
            return ReadNode(pointer.Short);
        }

        public IBPlusTreeNode AddValue(object value, Pointer valuePointer)
        {
            if (IsLeaf)
            {
                AddValueToSelf(new BPlusTreeNodeValue<TKeyType> { Pointer = valuePointer, Value = (TKeyType)value });
            }
            else
            {
                AddValueToChild((TKeyType)value, valuePointer);
            }

            return this;
        }

        private void Split()
        {
            if (IsRoot)
            {
                int half = (int)Math.Ceiling(Values.Count / (double)2);
                List<BPlusTreeNodeValue<TKeyType>> firstHalf = Values.Take(half).ToList();
                List<BPlusTreeNodeValue<TKeyType>> secondHalf = Values.Skip(half).ToList();

                BPlusTreeNode<TKeyType> leftChild = AddChild();
                leftChild.Values = firstHalf;
                leftChild.IsLeaf = IsLeaf;
         
                BPlusTreeNode<TKeyType> rightChild = AddChild();
                rightChild.IsLeaf = IsLeaf;
                rightChild.Values = secondHalf;

                BPlusTreeNodeValue<TKeyType> middle = new BPlusTreeNodeValue<TKeyType> { Value = Values[half].Value, LeftPointer = leftChild.Page, RightPointer = rightChild.Page };
                Values = new List<BPlusTreeNodeValue<TKeyType>> { middle };

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
                List<BPlusTreeNodeValue<TKeyType>> firstHalf = Values.Take(half).ToList();
                List<BPlusTreeNodeValue<TKeyType>> secondHalf = Values.Skip(half).ToList();

                BPlusTreeNode<TKeyType> rightChild = Parent.AddChild();
                rightChild.Values = secondHalf;

                Values = firstHalf;

                if (IsLeaf)
                {
                    Sibling = rightChild;
                    rightChild.IsLeaf = true;
                }

                Parent.AddValueToSelf(new BPlusTreeNodeValue<TKeyType> { Value = secondHalf.First().Value, LeftPointer = null, RightPointer = rightChild.Page });

                Dirty = true;
            }
        }

        public void AddValueToSelf(BPlusTreeNodeValue<TKeyType> value)
        {
            int target = -1;

            for (int i = 0; i < Values.Count; i++)
            {
                if (Compare(value.Value, Values[i].Value) < 0)
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

        public void AddValueToChild(TKeyType value, Pointer valuePointer)
        {
            BPlusTreeNode<TKeyType> target = ReadNode(GetTargetPointer(value));

            target.AddValue(value, valuePointer);
        }

        private Pointer GetTargetPointer(TKeyType value)
        {
            for (int i = Values.Count - 1; i >= 0; i--)
            {
                if (Compare(value, Values[i].Value) >= 0)
                {
                    return Values[i].RightPointer;
                }
            }

            return Values[0].LeftPointer;
        }

        private int Compare(object value1, object value2)
        {
            if (value1 is int leftInt && value2 is int rightInt)
            {
                return leftInt.CompareTo(rightInt);
            }
            else if (value1 is string left && value2 is string right)
            {
                string alphabet = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWw123456789";

                for(int i = 0; i < left.Length; i++)
                {
                    if (right.Length > i)
                    {
                        int indexLeft = alphabet.IndexOf(left[i]);
                        int indexRight = alphabet.IndexOf(right[i]);
                        if (indexRight > indexLeft)
                        {
                            return 1;
                        }
                        else if (indexLeft > indexRight)
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            return 0;
        }

        public BPlusTreeNode<TKeyType> AddChild()
        {
            BPlusTreeNode<TKeyType> child = new BPlusTreeNode<TKeyType>(Relation, StorageFile, StorageFile.GetFreeBlock(), NodeCache);
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
                        BPlusTreeNode<TKeyType> node = ReadNode(Values[i].LeftPointer);

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
                        BPlusTreeNode<TKeyType> node = ReadNode(Values[i].RightPointer);

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

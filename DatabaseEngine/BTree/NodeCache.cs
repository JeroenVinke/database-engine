using System;
using System.Collections.Generic;

namespace DatabaseEngine
{
    public class NodeCache<TKeyType> : Dictionary<uint, BPlusTreeNode<TKeyType>> where TKeyType : IComparable
    {
    }
}

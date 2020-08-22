using System;
using System.Collections.Generic;

namespace DatabaseEngine
{
    public class NodeCache<TKeyType> : Dictionary<int, BPlusTreeNode<TKeyType>> where TKeyType : IComparable
    {
    }
}

using System;

namespace DatabaseEngine.LogicalPlan
{
    public abstract class LogicalElement
    {
        public LogicalElement LeftChild { get; set; }
        public LogicalElement RightChild { get; set; }
        public int Id { get; set; }
        public static int MaxId { get; set; }

        public LogicalElement()
        {
            Id = ++MaxId;
        }

        public LogicalElement(LogicalElement child) : this()
        {
            LeftChild = child;
        }

        public LogicalElement(LogicalElement leftChild, LogicalElement rightChild) : this()
        {
            LeftChild = leftChild;
            RightChild = rightChild;
        }

        public abstract string Stringify();

        public virtual string ToDot()
        {

            string dot = "";

            dot = $"\rgroup_" + Id + " ["
                   + "\rlabel =<" + Stringify() + ">]";

            if (LeftChild != null && RightChild != null)
            {
                dot += LeftChild.ToDot();
                dot += RightChild.ToDot();

                dot += "\rgroup_" + Id + " -> " + "group_" + LeftChild.Id;
                dot += "\rgroup_" + Id + " -> " + "group_" + RightChild.Id;
                dot += "\r{rank=same group_" + LeftChild.Id + " group_" + RightChild.Id + "}";
            } else if (LeftChild != null)
            {
                dot += LeftChild.ToDot();

                dot += "\rgroup_" + Id + " -> group_" + LeftChild.Id;
            } else if (RightChild != null)
            {
                dot += RightChild.ToDot();

                dot += "\rgroup_" + Id + " -> group_" + RightChild.Id;
            }

            return dot;
        }

    }
}

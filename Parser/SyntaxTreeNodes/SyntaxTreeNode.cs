using System.Collections.Generic;
using System.Linq;

namespace Compiler.Parser
{
    public abstract class SyntaxTreeNode
    {
        public SyntaxTreeNodeType Type { get; set; }
        public int Id { get; set; }
        public static int MaxId { get; set; }

        public SyntaxTreeNode(SyntaxTreeNodeType type)
        {
            Type = type;
            Id = MaxId++;
        }

        public virtual string ToDot()
        {
            string result = $"{Id}2222 [label=\"{ToString()}\"]\r\n";

            foreach(SyntaxTreeNode child in GetChildren().Where(x => x != null))
            {
                result += $"{child.Id}2222 -> {Id}2222\r\n";
                result += child.ToDot();
            }

            return result;
        }

        protected virtual List<SyntaxTreeNode> GetChildren()
        {
            return new List<SyntaxTreeNode>();
        }
    }
}

namespace Compiler.Parser.SyntaxTreeNodes
{
    public class IdentifierASTNode : FactorASTNode
    {
        //public SymbolTableEntry SymbolTableEntry { get; set; }

        public IdentifierASTNode() : base(SyntaxTreeNodeType.Identifier)
        {
        }

        public string Identifier { get; internal set; }

        public override string ToString()
        {
            return Identifier;
        }
    }
}

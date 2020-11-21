namespace DatabaseEngine.LogicalPlan
{
    public class MemorySetElement : ReadLogicalElement
    {
        public Set Set { get; set; }
        public TableDefinition TableDefinition { get; set; }

        public MemorySetElement(TableDefinition tableDefinition, Set set)
        {
            Set = set;
            TableDefinition = tableDefinition;
        }

        public override int T()
        {
            return Set.Count();
        }

        public override double V(AttributeDefinition column)
        {
            throw new System.Exception();
        }

        public override string Stringify()
        {
            return "Set";
        }
    }
}

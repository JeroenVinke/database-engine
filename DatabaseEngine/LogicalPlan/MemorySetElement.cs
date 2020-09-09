namespace DatabaseEngine.LogicalPlan
{
    public class MemorySetElement : LogicalElement
    {
        public Set Set { get; set; }
        public TableDefinition TableDefinition { get; set; }

        public MemorySetElement(TableDefinition tableDefinition, Set set)
        {
            Set = set;
            TableDefinition = tableDefinition;
        }

        public override string Stringify()
        {
            return "Set";
        }
    }
}

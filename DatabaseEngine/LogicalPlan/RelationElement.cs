namespace DatabaseEngine.LogicalPlan
{
    public class RelationElement : ReadLogicalElement
    {
        public Relation Relation { get; set; }

        public RelationElement(Relation relation)
        {
            Relation = relation;
        }

        public override int T()
        {
            return Program.StatisticsManager.T(Relation as TableDefinition);
        }

        public override double V(AttributeDefinition column)
        {
            return Program.StatisticsManager.V(Relation as TableDefinition, column);
        }

        public override string Stringify()
        {
            return "RELATION(" + Relation.Name + ")";
        }
    }
}

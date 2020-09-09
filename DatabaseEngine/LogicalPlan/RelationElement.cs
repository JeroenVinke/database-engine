namespace DatabaseEngine.LogicalPlan
{
    public class RelationElement : LogicalElement
    {
        public Relation Relation { get; set; }

        public RelationElement(Relation relation)
        {
            Relation = relation;
        }

        public override string Stringify()
        {
            return "RELATION(" + Relation.Name + ")";
        }
    }
}

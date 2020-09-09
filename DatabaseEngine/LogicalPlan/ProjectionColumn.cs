namespace DatabaseEngine.LogicalPlan
{
    public class ProjectionColumn
    {
        public Relation Relation { get; set; }
        public AttributeDefinition AttributeDefinition { get; set; }
    }
}

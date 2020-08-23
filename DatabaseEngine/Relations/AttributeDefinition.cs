namespace DatabaseEngine
{
    public class AttributeDefinition
    {
        public string Name { get; set; }
        public ValueType Type { get; set; }
        public bool IsFixedSize
        {
            get
            {
                switch (Type)
                {
                    case ValueType.Integer:
                        return true;
                }

                return false;
            }
        }
    }
}

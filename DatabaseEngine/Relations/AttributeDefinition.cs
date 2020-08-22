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
        public int Size
        {
            get
            {
                switch(Type)
                {
                    case ValueType.String:
                        return 256;
                    case ValueType.Integer:
                        return 4;
                }

                return 0;
            }
        }
    }
}

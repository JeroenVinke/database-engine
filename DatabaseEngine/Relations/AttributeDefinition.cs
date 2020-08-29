using DatabaseEngine.Models;

namespace DatabaseEngine
{
    public class AttributeDefinition
    {
        [FromColumn("Name")]
        public string Name { get; set; }
        [FromColumn("Type")]
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

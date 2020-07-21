using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class TableDefinition : Relation
    {
        public string Name { get; set; }
        public int Id { get; internal set; }
        public int MaxRecordSize
        {
            get
            {
                return this.Sum(x => x.Size);
            }
        }
    }

    public class Relation : List<AttributeDefinition>
    {
    }

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

    public enum ValueType
    {
        String,
        Integer
    }
}

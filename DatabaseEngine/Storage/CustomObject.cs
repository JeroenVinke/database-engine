using System;
using System.Collections.Generic;

namespace DatabaseEngine
{
    public class CustomObject
    {
        public object Value { get; set; }
        public AttributeDefinition AttributeDefinition { get; set; }

        public bool IsEqualTo(CustomObject otherObject)
        {
            if (ReferenceEquals(otherObject.AttributeDefinition, AttributeDefinition))
            {
                CustomValueComparer comparer = Comparers.GetComparer(AttributeDefinition.Type);
                return comparer.IsEqualTo(Value, otherObject.Value);
            }

            return false;
        }

        public byte[] ToBytes()
        {
            switch (AttributeDefinition.Type)
            {
                case ValueType.String:
                    return System.Text.Encoding.UTF8.GetBytes((string)Value);
                case ValueType.Integer:
                    return BitConverter.GetBytes((int)Value);
            }

            return new byte[0];
        }

        internal static CustomObject FromBytes(byte[] entryBytes, AttributeDefinition attributeDefinition)
        {
            CustomObject obj = new CustomObject()
            {
                AttributeDefinition = attributeDefinition
            };

            switch (attributeDefinition.Type)
            {
                case ValueType.String:
                    obj.Value = System.Text.Encoding.UTF8.GetString(entryBytes);
                    break;
                case ValueType.Integer:
                    obj.Value = BitConverter.ToInt32(entryBytes, 0);
                    break;
            }

            return obj;
        }
    }

    public static class Comparers
    {
        private static Dictionary<ValueType, CustomValueComparer> comparers =
            new Dictionary<ValueType, CustomValueComparer>()
            {
                { ValueType.String, new StringValueComparer() },
                { ValueType.Integer, new IntegerValueComparer() }
            };

        public static CustomValueComparer GetComparer(ValueType type)
        {
            return comparers[type];
        }
    }

    public abstract class CustomValueComparer
    {
        public abstract bool IsEqualTo(object value, object otherValue);
    }

    public class StringValueComparer : CustomValueComparer
    {
        public override bool IsEqualTo(object value, object otherValue)
        {
            return string.Equals((string)value, (string)otherValue, System.StringComparison.Ordinal);
        }
    }

    public class IntegerValueComparer : CustomValueComparer
    {
        public override bool IsEqualTo(object value, object otherValue)
        {
            return (int)value == (int)otherValue;
        }
    }
}
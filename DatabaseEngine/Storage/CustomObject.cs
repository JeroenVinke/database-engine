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
            CustomValueComparer comparer = Comparers.GetComparer(AttributeDefinition.Type);
            return comparer.IsEqualTo(Value, otherObject.Value);
        }

        public bool IsEqualTo(object otherValue)
        {
            CustomValueComparer comparer = Comparers.GetComparer(AttributeDefinition.Type);
            return comparer.IsEqualTo(Value, otherValue);
        }

        public bool IsGreaterThan(object otherValue)
        {
            CustomValueComparer comparer = Comparers.GetComparer(AttributeDefinition.Type);
            return comparer.IsGreaterThan(Value, otherValue);
        }

        public byte[] ToBytes()
        {
            switch (AttributeDefinition.Type)
            {
                case ValueType.String:
                    return System.Text.Encoding.UTF8.GetBytes((string)Value);
                case ValueType.Integer:
                    return BitConverter.GetBytes((int)Value);
                case ValueType.Boolean:
                    return BitConverter.GetBytes((bool)Value);
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
                case ValueType.Boolean:
                    obj.Value = BitConverter.ToBoolean(entryBytes, 0);
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
                { ValueType.Integer, new IntegerValueComparer() },
                { ValueType.Boolean, new BooleanValueComparer() }
            };

        public static CustomValueComparer GetComparer(ValueType type)
        {
            return comparers[type];
        }
    }

    public abstract class CustomValueComparer
    {
        public abstract bool IsEqualTo(object value, object otherValue);
        public abstract int Compare(object value, object otherValue);

        public virtual bool IsGreaterThan(object value, object otherValue)
        {
            return false;
        }
    }

    public class StringValueComparer : CustomValueComparer
    {
        public override int Compare(object value, object otherValue)
        {
            string alphabet = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWw123456789";
            string left = (string)value;
            string right = (string)otherValue;

            for (int i = 0; i < left.Length; i++)
            {
                if (right.Length > i)
                {
                    int indexLeft = alphabet.IndexOf(left[i]);
                    int indexRight = alphabet.IndexOf(right[i]);
                    if (indexRight > indexLeft)
                    {
                        return 1;
                    }
                    else if (indexLeft > indexRight)
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }

            return  0;
        }

        public override bool IsEqualTo(object value, object otherValue)
        {
            return string.Equals((string)value, (string)otherValue, System.StringComparison.Ordinal);
        }

        public override bool IsGreaterThan(object value, object otherValue)
        {
            return Compare(value, otherValue) > 0;
        }
    }

    public class IntegerValueComparer : CustomValueComparer
    {
        public override int Compare(object value, object otherValue)
        {
            return ((int)value).CompareTo((int)otherValue);
        }

        public override bool IsEqualTo(object value, object otherValue)
        {
            return (int)value == (int)otherValue;
        }

        public override bool IsGreaterThan(object value, object otherValue)
        {
            return Compare(value, otherValue) > 0;
        }
    }

    public class BooleanValueComparer : CustomValueComparer
    {
        public override int Compare(object value, object otherValue)
        {
            return ((bool)value == (bool)otherValue) ? 0 : -1;
        }

        public override bool IsEqualTo(object value, object otherValue)
        {
            return (bool)value == (bool)otherValue;
        }
    }
}
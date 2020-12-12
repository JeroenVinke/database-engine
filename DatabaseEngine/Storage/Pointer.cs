using System.Collections;
using System.Diagnostics;

namespace DatabaseEngine
{
    [DebuggerDisplay("PageNumber: {PageNumber}, Index: {Index}, Short: {Short}")]
    public class Pointer : IEqualityComparer
    {
        public uint PageNumber { get; set; }
        public uint Index { get; set; }
        public uint Short
        {
            get
            {
                return (uint)((PageNumber << 16) ^ Index);
            }
        }

        public Pointer(uint pageNumber, uint index)
        {
            PageNumber = pageNumber;
            Index = index;
        }

        public Pointer(uint value)
        {
            PageNumber = value >> 16;

            Index = value & 0x00FF;
        }

        public new bool Equals(object x, object y)
        {
            return ((Pointer)x).Short == ((Pointer)y).Short;
        }

        public int GetHashCode(object obj)
        {
            return (int)((Pointer)obj).Short;
        }
    }
}

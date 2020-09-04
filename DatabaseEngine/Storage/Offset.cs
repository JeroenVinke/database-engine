using System;

namespace DatabaseEngine
{
    public class Offset
    {
        public ushort Bytes { get; set; }

        public Offset(ushort bytes)
        {
            Bytes = bytes;
        }

        public Offset()
        {
        }

        public virtual int Size => 2;

        public virtual byte[] GetOffsetInBytes()
        {
            byte[] result = BitConverter.GetBytes(Bytes);
            return result;
        }
    }
}

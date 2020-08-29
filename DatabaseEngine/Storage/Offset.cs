using System;

namespace DatabaseEngine
{
    public class Offset
    {
        public int Bytes { get; set; }

        public Offset(int bytes)
        {
            Bytes = bytes;
        }

        public Offset()
        {
        }

        public virtual int Size => 4;

        public virtual byte[] GetOffsetInBytes()
        {
            byte[] result = BitConverter.GetBytes(Bytes);
            return result;
        }
    }
}

using System;

namespace DatabaseEngine
{
    public class Offset
    {
        public int Bytes { get; set; }

        internal byte[] GetOffsetInBytes()
        {
            return BitConverter.GetBytes((ushort)(Bytes));
        }
    }
}

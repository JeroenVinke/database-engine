using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseEngine
{
    public class Pointer
    {
        public int PageNumber { get; set; }
        public int Index { get; set; }

        public Pointer(int pageNumber, int index)
        {

            PageNumber = pageNumber;
            Index = index;
        }

        public long GetPointerAsLong()
        {
            return (PageNumber << 4) ^ Index;
        }

        public static Pointer GetPointerFromLong(long value)
        {
            Pointer pointer = new Pointer((int)((value >> 32) & 0x0000FFFF), (int)(value & 0x0000FFFF));

            return pointer;
        }
    }
}

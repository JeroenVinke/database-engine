namespace DatabaseEngine
{
    public class Pointer
    {
        public int PageNumber { get; set; }
        public int Index { get; set; }
        public short Short
        {
            get
            {
                return (short)((PageNumber << 8) ^ Index);
            }
        }

        public Pointer(int pageNumber, int index)
        {
            PageNumber = pageNumber;
            Index = index;
        }

        public Pointer(short value)
        {
            PageNumber = (int)((value >> 8));

            Index = (int)(value & 0x00FF);
        }
    }
}

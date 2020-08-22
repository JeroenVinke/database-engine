namespace DatabaseEngine
{
    public class Pointer
    {
        public int PageNumber { get; set; }
        public int Index { get; set; }
        public int Short
        {
            get
            {
                return (int)((PageNumber << 16) ^ Index);
            }
        }

        public Pointer(int pageNumber, int index)
        {
            PageNumber = pageNumber;
            Index = index;
        }

        public Pointer(int value)
        {
            PageNumber = (int)((value >> 16));

            Index = (int)(value & 0x00FF);
        }
    }
}

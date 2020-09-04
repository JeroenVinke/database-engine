namespace DatabaseEngine
{
    public class Pointer
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
    }
}

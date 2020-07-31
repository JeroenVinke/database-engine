namespace DatabaseEngine
{
    public abstract class Record
    {
        public abstract int Length { get; }
        
        public byte[] Content { get; set; }

        public abstract byte[] ToBytes();
    }
}

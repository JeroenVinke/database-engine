namespace DatabaseEngine
{
    public abstract class Record
    {
        public abstract int Length { get; }

        public abstract byte[] ToBytes();
    }
}

using System;

namespace DatabaseEngine
{
    public class DataBlockHeader : BlockHeader
    {
        public override BlockType Type => BlockType.Data;

        public int RelationId { get; set; }

        public DataBlockHeader(BlockBuffer buffer)
            : base(buffer)
        {
        }

        public DataBlockHeader()
        {
        }

        public static DataBlockHeader CreateDataHeader(BlockBuffer buffer)
        {
            DataBlockHeader header = new DataBlockHeader(buffer);

            return header;
        }

        protected override void ReadCustomHeaderFromBuffer(BlockBuffer buffer)
        {
            RelationId = BitConverter.ToInt32(buffer.ReadBytes(4));
        }

        protected override byte[] GetCustomHeaderBytes()
        {
            return BitConverter.GetBytes(RelationId);
        }
    }
}
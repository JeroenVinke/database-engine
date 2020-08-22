using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public abstract class Block
    {
        // header
        public BlockHeader Header { get; set; }
        public List<Record> Records { get; set; } = new List<Record>();

        public Block NextBlock { get; set; } // todo: overflow

        public const int BlockSize = 4096;

        public Block()
        {
        }

        internal int AddRecord(Record record)
        {
            int targetPosition = DetermineOffsetPositionForRecord(record);

            int offsetIndex;
            if (Header.Offsets.Count == 0)
            {
                offsetIndex = BlockSize - record.Length;
            }
            else
            {
                offsetIndex = Header.Offsets.Select(x => x.Bytes).Min() - record.Length;
            }

            Offset offset = new Offset() { Bytes = offsetIndex };
            Header.Offsets.Insert(targetPosition, offset);
            record.OffsetInBlock = offset;
            Records.Add(record);
            return targetPosition;
        }

        protected virtual int DetermineOffsetPositionForRecord(Record record)
        {
            return Records.Count;
        }

        public byte[] ToBytes()
        {
            byte[] headerBytes = Header.ToBytes().ToArray();

            byte[] byteArray = new byte[BlockSize];
            headerBytes.CopyTo(byteArray, 0);

            int i = BlockSize;
            foreach (Record record in Records)
            {
                byte[] recordBytes = record.ToBytes();

                recordBytes.CopyTo(byteArray, i - record.Length);

                i -= record.Length;
            }

            return byteArray;
        }

        public static IndexBlock CreateIndexBlockFromBuffer(byte[] buffer)
        {
            IndexBlock block = new IndexBlock();
            block.Header = IndexBlockHeader.CreateIndexHeader(new BlockBuffer(buffer));

            List<Record> records = new List<Record>();

            List<Offset> orderedOffsets = block.Header.Offsets.OrderBy(x => x.Bytes).ToList();

            for (int i = 0; i < block.Header.Offsets.Count; i++)
            {
                Offset offset = block.Header.Offsets[i];
                Offset nextOffset = orderedOffsets.Where(x => x.Bytes > offset.Bytes).FirstOrDefault();

                byte[] recordBytes;

                if (nextOffset != null)
                {
                    recordBytes = buffer.Skip(offset.Bytes).Take(nextOffset.Bytes - offset.Bytes).ToArray();
                }
                else
                {
                    recordBytes = buffer.Skip(block.Header.Offsets[i].Bytes).ToArray();
                }

                Record record = new Record(recordBytes);
                records.Add(record);
            }

            block.Records = records.Cast<Record>().ToList();

            return block;
        }

        public static DataBlock CreateDataBlockFromBuffer(byte[] buffer)
        {
            DataBlock block = new DataBlock();
            block.Header = DataBlockHeader.CreateDataHeader(new BlockBuffer(buffer));

            List<Record> records = new List<Record>();

            List<Offset> orderedOffsets = block.Header.Offsets.OrderBy(x => x.Bytes).ToList();

            for (int i = 0; i < block.Header.Offsets.Count; i++)
            {
                Offset offset = block.Header.Offsets[i];
                Offset nextOffset = orderedOffsets.Where(x => x.Bytes > offset.Bytes).FirstOrDefault();

                byte[] recordBytes;

                if (nextOffset != null)
                {
                    recordBytes = buffer.Skip(offset.Bytes).Take(nextOffset.Bytes - offset.Bytes).ToArray();
                }
                else
                {
                    recordBytes = buffer.Skip(block.Header.Offsets[i].Bytes).ToArray();
                }

                Record record = new Record(recordBytes);
                record.OffsetInBlock = offset;
                records.Add(record);
            }

            block.Records = records.Cast<Record>().ToList();
            block.Relation = Program.Relations.First(x => x.Id == ((DataBlockHeader)block.Header).RelationId);

            return block;
        }

        public static DataBlock CreateDataBlock(Set items)
        {
            DataBlock block = new DataBlock();
            block.Relation = items.Relation;

            foreach (CustomTuple tuple in items.All())
            {
                Record record = tuple.ToRecord();

                block.AddRecord(record);
            }

            return block;
        }
    }
}

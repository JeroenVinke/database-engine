using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class Block
    {
        public Pointer Page { get; private set; }
        // header
        public BlockHeader Header { get; set; }
        public List<Record> Records { get; set; } = new List<Record>();

        private Block _nextBlock;
        public Block NextBlock
        { 
            get
            {
                if (_nextBlock == null && Header.NextBlockId != null && Header.NextBlockId.Short > 0)
                {
                    _nextBlock = _storageFile.ReadBlock(Relation, Header.NextBlockId);
                }

                return _nextBlock;
            }
        }

        private StorageFile _storageFile;

        public const int BlockSize = 4096;
        private int _usedBytes;

        private Relation _relation;
        public Relation Relation
        {
            get => _relation;
            set
            {
                _relation = value;
                Header.RelationId = value.Id;
            }
        }

        public Block(StorageFile storageFile, Relation relation, byte[] buffer, Pointer pageNumber)
        {
            _storageFile = storageFile;
            Page = pageNumber;
            Header = new BlockHeader(new BlockBuffer(buffer));
            Relation = relation;

            if (!Header.Empty)
            {
                List<Record> records = new List<Record>();

                List<Offset> orderedOffsets = Header.Offsets.OrderBy(x => x.Bytes).ToList();

                for (int i = 0; i < Header.Offsets.Count; i++)
                {
                    Offset offset = Header.Offsets[i];
                    Offset nextOffset = orderedOffsets.Where(x => x.Bytes > offset.Bytes).FirstOrDefault();

                    byte[] recordBytes;

                    if (nextOffset != null)
                    {
                        recordBytes = buffer.Skip(offset.Bytes).Take(nextOffset.Bytes - offset.Bytes).ToArray();
                    }
                    else
                    {
                        recordBytes = buffer.Skip(Header.Offsets[i].Bytes).ToArray();
                    }

                    Record record = new Record(relation, recordBytes);
                    record.OffsetInBlock = offset;
                    records.Add(record);
                }

                Records = records.Cast<Record>().ToList();
            }

            _usedBytes = Header.Size + Records.Sum(x => x.Length);
        }

        public Block(StorageFile storageFile, Relation relation)
        {
            _storageFile = storageFile;
            Header = new BlockHeader();
            Relation = relation;
        }

        protected int DetermineOffsetPositionForRecord(Record record)
        {
            int targetPosition = Header.Offsets.Count;

            if (Relation is TableDefinition tableDefinition && tableDefinition.HasClusteredIndex())
            {
                string clusteredIndex = (Relation as TableDefinition).GetClusteredIndex().Columns.First().Name;

                if (!string.IsNullOrEmpty(clusteredIndex) && Header.Offsets.Count > 0)
                {
                    CustomTuple tuple = new CustomTuple(Relation).FromRecord(record);
                    int idToInsert = tuple.GetValueFor<int>(clusteredIndex);

                    targetPosition = 0;

                    foreach (Offset offset1 in Header.Offsets)
                    {
                        Record dataRecord1 = GetRecordFromOffset(offset1);
                        CustomTuple t = new CustomTuple(Relation).FromRecord(dataRecord1);
                        int id = t.GetValueFor<int>(clusteredIndex);

                        if (id > idToInsert)
                        {
                            break;
                        }
                        else
                        {
                            targetPosition++;
                        }
                    }
                }

                return targetPosition;
            }

            return Records.Count;
        }

        private Record GetRecordFromOffset(Offset offset)
        {
            return Records.First(x => x.OffsetInBlock.Bytes == offset.Bytes);
        }

        public Set GetSet()
        {
            Set set = new Set(Relation);
            foreach (Record record in Records)
            {
                CustomTuple tuple = new CustomTuple(Relation);
                tuple.FromRecord(record);

                set.Add(tuple);
            }

            return set;
        }

        public int AddRecord(Record record)
        {
            Header.Empty = false;

            if (NextBlock != null || (BlockSize - _usedBytes) < (record.Length + 4))
            {
                if (NextBlock == null)
                {
                    Header.NextBlockId = _storageFile.GetFreeBlock();
                }
                return NextBlock.AddRecord(record);
            }

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

            _usedBytes += record.Length;
            return targetPosition;
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
    }
}

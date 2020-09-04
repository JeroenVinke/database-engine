using DatabaseEngine.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class Block
    {
        public Pointer Page { get; set; }
        // header
        public BlockHeader Header { get; set; }
        private List<Record> _records { get; set; } = new List<Record>();
        private List<Record> _sortedRecords { get; set; } = new List<Record>();
        private Block _nextBlock;
        public Block NextBlock
        { 
            get
            {
                if (_nextBlock == null && Header.NextBlockId != null && Header.NextBlockId.Short > 0)
                {
                    _nextBlock = _memoryManager.Read(Relation, Header.NextBlockId);
                }

                return _nextBlock;
            }
        }

        private MemoryManager _memoryManager;

        public const int BlockSize = 4096;

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

        public int UsedBytes => Header.Size + _records.Sum(x => x.Length);

        public Record GetRecordForRowId(uint index)
        {
            if (!Header.Offsets.Any(x => x.Id == index) && NextBlock != null)
            {
                return NextBlock.GetRecordForRowId(index - Header.Offsets.Max(x => x.Id));
            }
            return _sortedRecords[Header.Offsets.IndexOf(Header.Offsets.First(x => x.Id == index))];
        }

        public Block(MemoryManager memoryManager, Relation relation, byte[] buffer, Pointer pageNumber)
        {
            _memoryManager = memoryManager;
            Page = pageNumber;
            Header = new BlockHeader(new BlockBuffer(buffer));
            Relation = relation;

            if (Header.IsFilled)
            {
                Record[] sortedRecords = new Record[Header.Offsets.Count];

                int end = BlockSize;
                foreach(RecordOffset offset in Header.Offsets.OrderByDescending(x => x.Bytes))
                {
                    byte[] recordBytes = buffer.Skip(offset.Bytes).Take(end - offset.Bytes).ToArray();
                    end -= recordBytes.Length;

                    Record record = new Record(relation, recordBytes);
                    _records.Insert(0, record);
                    sortedRecords[Header.Offsets.IndexOf(offset)] = record;
                }

                _sortedRecords = sortedRecords.ToList();
            }
        }


        public Block(MemoryManager memoryManager, Relation relation)
        {
            _memoryManager = memoryManager;
            Header = new BlockHeader();
            Relation = relation;
        }

        protected int DetermineOffsetPositionForRecord(Record record)
        {
            int targetPosition = Header.Offsets.Count;

            if (Relation is TableDefinition tableDefinition && tableDefinition.HasClusteredIndex())
            {
                string clusteredIndex = (Relation as TableDefinition).GetClusteredIndex().Column;

                if (!string.IsNullOrEmpty(clusteredIndex) && Header.Offsets.Count > 0)
                {
                    CustomTuple tuple = new CustomTuple(Relation).FromRecord(record);
                    int idToInsert = tuple.GetValueFor<int>(clusteredIndex);

                    foreach(CustomTuple existingTuple in _records.Select(x => new CustomTuple(Relation).FromRecord(x)))
                    {
                        if (idToInsert < existingTuple.GetValueFor<int>(clusteredIndex))
                        {
                            targetPosition --;
                        }
                        else
                        {
                            return targetPosition;
                        }
                    }
                }
            }

            return targetPosition;
        }

        public Set GetSet()
        {
            Set set = new Set(Relation);
            foreach (Record record in _sortedRecords)
            {
                CustomTuple tuple = new CustomTuple(Relation);
                tuple.FromRecord(record);

                set.Add(tuple);
            }

            return set;
        }

        public (Pointer, Block) AddRecord(Record record)
        {
            Header.IsFilled = true;

            if (NextBlock != null || (BlockSize - (UsedBytes + record.Length + new RecordOffset(0, 0).Size)) <= 0)
            {
                if (NextBlock == null)
                {
                    Header.NextBlockId = _memoryManager.GetFreeBlock();
                    _nextBlock = new Block(_memoryManager, Relation);
                    _nextBlock.Page = Header.NextBlockId;
                }
                return NextBlock.AddRecord(record);
            }

            int targetPosition = DetermineOffsetPositionForRecord(record);

            ushort offsetIndex;
            if (Header.Offsets.Count == 0)
            {
                offsetIndex = (ushort)(BlockSize - record.Length);
            }
            else
            {
                offsetIndex = (ushort)(Header.Offsets.Select(x => x.Bytes).Min() - record.Length);
            }

            RecordOffset offset = new RecordOffset((ushort)offsetIndex, Header.Offsets.Count > 0 ? (ushort)(Header.Offsets.Max(x => x.Id) + 1) : (ushort)0);
            Header.Offsets.Insert(targetPosition, offset);
            _records.Insert(0, record);
            _sortedRecords.Insert(targetPosition, record);

            CheckMaxSize();

            return (new Pointer(Page.PageNumber, offset.Id), this);
        }

        public List<Record> GetSortedRecords()
        {
            return _sortedRecords;
        }

        private void CheckMaxSize()
        {
            var x = UsedBytes;
            if (x >= BlockSize)
            {
                throw new Exception();
            }
        }

        public byte[] ToBytes()
        {
            CheckMaxSize();

            byte[] headerBytes = Header.ToBytes().ToArray();

            byte[] byteArray = new byte[BlockSize];
            headerBytes.CopyTo(byteArray, 0);

            int i = BlockSize;
            List<Record> reversedRecords = _records.ToList();
            reversedRecords.Reverse();
            foreach (Record record in reversedRecords)
            {
                byte[] recordBytes = record.ToBytes();

                recordBytes.CopyTo(byteArray, i - record.Length);

                i -= record.Length;
            }

            return byteArray;
        }

        public void Write()
        {
            _memoryManager.Persist(this);
        }
    }
}

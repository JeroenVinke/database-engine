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

        public Record GetRecordForRowId(int index)
        {
            return _sortedRecords[Header.Offsets.IndexOf(Header.Offsets.First(x => x.Id == index))];
        }

        public Block(StorageFile storageFile, Relation relation, byte[] buffer, Pointer pageNumber)
        {
            _storageFile = storageFile;
            Page = pageNumber;
            Header = new BlockHeader(new BlockBuffer(buffer));
            Relation = relation;

            if (!Header.Empty)
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

            _usedBytes = Header.Size + _records.Sum(x => x.Length);
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

        public Pointer AddRecord(Record record)
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

            RecordOffset offset = new RecordOffset(offsetIndex, Header.Offsets.Count > 0 ? Header.Offsets.Max(x => x.Id) + 1 : 0);
            Header.Offsets.Insert(targetPosition, offset);
            _records.Insert(0, record);
            _sortedRecords.Insert(targetPosition, record);

            Write();

            _usedBytes += record.Length + offset.Size;
            return new Pointer(Page.PageNumber, offset.Id);
        }

        public List<Record> GetSortedRecords()
        {
            return _sortedRecords;
        }

        public byte[] ToBytes()
        {
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
            _storageFile.WriteBlock(Page.PageNumber, ToBytes());
        }
    }
}

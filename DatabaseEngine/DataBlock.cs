using System;
using System.Linq;

namespace DatabaseEngine
{
    public class DataBlock : Block
    {
        private Relation _relation;
        public Relation Relation
        {
            get => _relation;
            set
            {
                _relation = value;
                ((DataBlockHeader)Header).RelationId = value.Id;
            }
        }

        public DataBlock() : base()
        {
            Header = new DataBlockHeader();
        }

        public override void OnOffsetDetermined(Record record, Offset offset)
        {
            base.OnOffsetDetermined(record, offset);

            ((DataRecord)record).Offset = offset;
        }

        protected override int DetermineOffsetPositionForRecord(Record record)
        {
            int targetPosition = Header.Offsets.Count;

            string clusteredIndex = (Relation as TableDefinition).ClusteredIndex;

            if (!string.IsNullOrEmpty(clusteredIndex) && Header.Offsets.Count > 0)
            {
                CustomTuple tuple = new CustomTuple(Relation).FromRecord((DataRecord)record);
                int idToInsert = (int)tuple.GetValueFor(clusteredIndex);

                targetPosition = 0;

                foreach (Offset offset1 in Header.Offsets)
                {
                    DataRecord dataRecord1 = GetRecordFromOffset(offset1);
                    CustomTuple t = new CustomTuple(Relation).FromRecord(dataRecord1);
                    int id = (int)t.GetValueFor(clusteredIndex);

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

        private DataRecord GetRecordFromOffset(Offset offset)
        {
            return (DataRecord)Records.First(x => ((DataRecord)x).Offset.Bytes == offset.Bytes);
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
    }
}

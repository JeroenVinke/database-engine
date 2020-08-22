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

        protected override int DetermineOffsetPositionForRecord(Record record)
        {
            int targetPosition = Header.Offsets.Count;

            string clusteredIndex = (Relation as TableDefinition).GetClusteredIndex().Columns.First().Name;

            if (!string.IsNullOrEmpty(clusteredIndex) && Header.Offsets.Count > 0)
            {
                CustomTuple tuple = new CustomTuple(Relation).FromRecord(record);
                int idToInsert = (int)tuple.GetValueFor(clusteredIndex);

                targetPosition = 0;

                foreach (Offset offset1 in Header.Offsets)
                {
                    Record dataRecord1 = GetRecordFromOffset(offset1);
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

        private Record GetRecordFromOffset(Offset offset)
        {
            return (Record)Records.First(x => ((Record)x).OffsetInBlock.Bytes == offset.Bytes);
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

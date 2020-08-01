using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static DatabaseEngine.StorageFile;

namespace DatabaseEngine
{
    public class Block
    {
        [DllImport("kernel32.dll")]
        public static extern bool WriteFileEx(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            [In] ref System.Threading.NativeOverlapped lpOverlapped,
            WriteFileCompletionDelegate lpCompletionRoutine);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            ref NativeOverlapped lpOverlapped
        );

        // header
        public BlockHeader Header { get; set; }
        public List<Record> Records { get; set; } = new List<Record>();

        public Block NextBlock { get; set; } // overflow
        public Relation Relation { get; set; }

        public int Size { get; internal set; }
        public const int BlockSize = 4096;

        internal void AddRecord(Record record)
        {
            Offset last = Header.Offsets.FirstOrDefault();
            int offset;
            if (last == null)
            {
                offset = Size - record.Length;
            }
            else
            {
                offset = last.Bytes - record.Length;
            }
            Header.Offsets.Insert(0, new Offset() { Bytes = offset });
            Records.Add(record);
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>(Size);
            bytes.AddRange(Header.ToBytes());

            byte[] byteArray = bytes.ToArray();
            int i = Size;
            foreach(Record record in Records)
            {
                byte[] recordBytes = record.ToBytes();

                recordBytes.CopyTo(byteArray, i - recordBytes.Length);

                i -= recordBytes.Length;
            }

            return byteArray;
        }

        public static Block CreateIndexBlockFromBuffer(BlockBuffer buffer)
        {
            Block block = new Block();
            block.Header = BlockHeader.CreateHeader(buffer);

            List<IndexRecord> records = new List<IndexRecord>();

            for (int i = block.Header.Offsets.Count - 1; i >= 0; i--)
            {
                byte[] recordBytes = buffer.ReadBytes(block.Header.Offsets[i].Bytes).ToArray();

                IndexRecord record = IndexRecord.FromBytes(recordBytes);
                records.Add(record);
            }

            block.Records = records.Cast<Record>().ToList();

            return block;
        }

        public static Block CreateDataBlockFromBuffer(BlockBuffer buffer, TableDefinition tableDefinition)
        {
            Block block = new Block();
            block.Header = BlockHeader.CreateHeader(buffer);

            List<DataRecord> records = new List<DataRecord>();

            for (int i = block.Header.Offsets.Count - 1; i >= 0; i--)
            {
                byte[] recordBytes = buffer.ReadBytes(block.Header.Offsets[i].Bytes).ToArray();

                DataRecord record = DataRecord.FromBytes(recordBytes);
                records.Add(record);
            }

            block.Records = records.Cast<Record>().ToList();
            block.Relation = tableDefinition;

            return block;
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

        public static Block CreateDataBlock(Set items)
        {
            Block block = new Block();
            block.Size = Block.BlockSize;
            block.Header = new DataBlockHeader(null);

            foreach (CustomTuple tuple in items.All())
            {
                Record record = tuple.ToRecord();

                block.AddRecord(record);
            }

            return block;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static DatabaseEngine.Program;

namespace DatabaseEngine
{
    public class Block
    {
        [DllImport("kernel32.dll")]
        static extern bool WriteFileEx(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            [In] ref System.Threading.NativeOverlapped lpOverlapped,
            WriteFileCompletionDelegate lpCompletionRoutine);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadFile(
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
        public static int BlockSize = 4096;

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

        public static Block CreateBlockFromBuffer(byte[] data, TableDefinition tableDefinition)
        {
            BlockBuffer buffer = new BlockBuffer(data);

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

        public void Write(IntPtr fileHandle)
        {
            byte[] blockBytes = ToBytes();

            NativeOverlapped overlapped = new NativeOverlapped();
            overlapped.OffsetLow = 0;
            WriteFileEx(fileHandle, blockBytes, (uint)blockBytes.Length, ref overlapped, Completed);
        }

        private static void Completed(uint dwErrorCode, uint dwNumberOfBytesTransfered, ref NativeOverlapped lpOverlapped)
        {
            ;
        }

        public static Block ReadBlock(IntPtr fileHandle, int fileOffset, TableDefinition tableDefinition)
        {
            NativeOverlapped overlapped = new NativeOverlapped();
            overlapped.OffsetLow = fileOffset;
            byte[] buffer = new byte[Block.BlockSize];
            uint readBytes;
            ReadFile(fileHandle, buffer, (uint)Block.BlockSize, out readBytes, ref overlapped);

            return CreateBlockFromBuffer(buffer, tableDefinition);
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

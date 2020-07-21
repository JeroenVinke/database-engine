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
        public List<Offset> Offsets { get; set; } = new List<Offset>();
        public List<Record> Records { get; set; } = new List<Record>();
        public int Size { get; internal set; }
        public static int BlockSize = 4096;
        public static int FixedHeaderSize = 0;
        public Relation Relation { get; set; }

        internal void AddRecord(Record record)
        {
            Offset last = Offsets.FirstOrDefault();
            int offset;
            if (last == null)
            {
                offset = Size - record.Length;
            }
            else
            {
                offset = last.Bytes - record.Length;
            }
            Offsets.Insert(0, new Offset() { Bytes = offset });
            Records.Add(record);
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[Size];
            int i = 0;
            foreach (Offset offset in Offsets)
            {
                byte[] offsetBytes = offset.GetOffsetInBytes();
                for(int ii = 0; ii < offsetBytes.Length; ii++)
                {
                    bytes[i + ii] = offsetBytes[ii];
                }
                i += 2;
            }

            

            i = Size;
            foreach(Record record in Records)
            {
                byte[] recordBytes = record.ToBytes();

                recordBytes.CopyTo(bytes, i - recordBytes.Length);

                i -= recordBytes.Length;
            }

            return bytes.ToArray();
        }

        public static Block CreateBlockFromBuffer(byte[] buffer, TableDefinition tableDefinition)
        {
            List<Offset> offsets = new List<Offset>();
            // todo, anders bepalen hoeveel offsets er zijn
            int maxRecords = (BlockSize - FixedHeaderSize) / tableDefinition.MaxRecordSize;
            for (int i = FixedHeaderSize; i < BlockSize; i += 2)
            {
                ushort offsetShort = BitConverter.ToUInt16(new byte[] { buffer[i], buffer[i + 1] }, 0);
                if (offsetShort > 0)
                {
                    Offset offset = new Offset()
                    {
                        Bytes = offsetShort
                    };
                    offsets.Add(offset);
                }
                else
                {
                    break;
                }
            }

            List<Record> records = new List<Record>();

            for (int i = 0; i < offsets.Count; i++)
            {
                Offset nextOffset = offsets.Count > i + 1 ? offsets[i + 1] : new Offset { Bytes = BlockSize };
                byte[] recordBytes = buffer.Skip(offsets[i].Bytes).Take(nextOffset.Bytes - offsets[i].Bytes).ToArray();

                Record record = Record.FromBytes(recordBytes);
                records.Add(record);
            }

            Block block = new Block();
            block.Offsets = offsets;
            block.Relation = tableDefinition;
            block.Records = records;

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

        public static Block CreateFromSet(Set products)
        {
            Block block = new Block();
            block.Size = Block.BlockSize;

            foreach (CustomTuple tuple in products.All())
            {
                Record record = tuple.ToRecord();

                block.AddRecord(record);
            }

            return block;
        }
    }

    public class Offset
    {
        public int Bytes { get; set; }

        internal byte[] GetOffsetInBytes()
        {
            return BitConverter.GetBytes((ushort)(Bytes));
        }
    }
}

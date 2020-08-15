using System;
using System.Collections.Generic;

namespace DatabaseEngine
{
    public class IndexRecord : Record
    {
        public override int Length
        {
            get
            {
                return 64;
            }
        }

        public int Value { get; set; }
        public Pointer Pointer { get; set; }
        public Pointer LeftPointer { get; set; }
        public Pointer RightPointer { get; set; }

        public static IndexRecord FromBytes(BlockBuffer bytes)
        {
            IndexRecord record = new IndexRecord();
            record.Value = BitConverter.ToInt32(bytes.ReadBytes(4));
            record.LeftPointer = GetPointer(BitConverter.ToInt16(bytes.ReadBytes(2)));
            record.Pointer = GetPointer(BitConverter.ToInt16(bytes.ReadBytes(2)));
            record.RightPointer = GetPointer(BitConverter.ToInt16(bytes.ReadBytes(2)));

            return record;
        }

        private static Pointer GetPointer(short v)
        {
            if (v == -1)
            {
                return null;
            }

            return new Pointer(v);
        }

        public override byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Value));
            if(LeftPointer != null)
            {
                bytes.AddRange(BitConverter.GetBytes(LeftPointer.Short));
            }
            else
            {
                bytes.AddRange(BitConverter.GetBytes((short)-1));
            }

            if (Pointer != null)
            {
                bytes.AddRange(BitConverter.GetBytes(Pointer.Short));
            }
            else
            {
                bytes.AddRange(BitConverter.GetBytes((short)-1));
            }

            if (RightPointer != null)
            {
                bytes.AddRange(BitConverter.GetBytes(RightPointer.Short));
            }
            else
            {
                bytes.AddRange(BitConverter.GetBytes((short)-1));
            }

            return bytes.ToArray();
        }
    }
}

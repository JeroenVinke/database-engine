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

        public static IndexRecord FromBytes(byte[] bytes)
        {
            IndexRecord record = new IndexRecord();
            record.Content = bytes;

            return record;
        }

        public override byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(Content);

            return bytes.ToArray();
        }
    }
}

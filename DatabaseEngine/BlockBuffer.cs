using System.Collections.Generic;

namespace DatabaseEngine
{
    public class BlockBuffer
    {
        private List<byte> _data;

        public BlockBuffer(byte[] data)
        {
            _data = new List<byte>(data);
        }

        public byte ReadByte()
        {
            byte b = _data[0];
            _data.RemoveAt(0);

            return b;
        }

        public byte[] ReadBytes(int count)
        {
            byte[] result = new byte[count];

            for(int i = 0; i < count;i++)
            {
                result[i] = ReadByte();
            }

            return result;
        }
    }
}

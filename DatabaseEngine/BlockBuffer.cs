using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine
{
    public class BlockBuffer
    {
        private List<byte> _data;
        public int Count => _data.Count;

        public BlockBuffer(byte[] data)
        {
            _data = new List<byte>(data);
        }

        public byte ReadByte()
        {
            return ReadByte(0);
        }

        public byte ReadByte(int index)
        {
            byte b = _data[index];
            _data.RemoveAt(index);

            return b;
        }

        public byte ReadLastByte()
        {
            byte b = _data[_data.Count - 1];
            _data.RemoveAt(_data.Count - 1);

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

        public byte[] ReadLastBytes(int count)
        {
            byte[] result = new byte[count];

            for (int i = count - 1; i >= 0; i--)
            {
                result[i] = ReadLastByte();
            }

            return result;
        }

        public byte[] ReadBytes(int startIndex, int count)
        {
            byte[] result = new byte[count];

            for (int i = startIndex + count - 1; i >= startIndex; i--)
            {
                result[i] = ReadByte(i);
            }

            return result;
        }

        public byte[] PeekBytes(int startIndex, int count)
        {
            byte[] result = new byte[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = _data[startIndex + i];
            }

            return result;
        }

        public byte[] PeekLastBytes(int count)
        {
            byte[] result = new byte[count];

            for (int i = 0; i < count; i++)
            {
                result[count - i - 1] = _data[_data.Count - i - 1];
            }

            return result;
        }
    }
}

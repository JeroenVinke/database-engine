using System;
using System.Drawing;
using System.Threading;

namespace DatabaseEngine
{
    public class FileHeader
    {
        private StorageFile _storageFile;

        public const int DefaultFirstBlock = 5;
        public uint FirstFreeBlock { get; set; } = DefaultFirstBlock;

        public FileHeader(StorageFile storageFile)
        {
            _storageFile = storageFile;

            BlockBuffer buffer = Read();
            FirstFreeBlock = BitConverter.ToUInt32(buffer.ReadBytes(4));

            if (FirstFreeBlock == 0)
            {
                FirstFreeBlock = DefaultFirstBlock;
            }
        }

        private BlockBuffer Read()
        {
            return new BlockBuffer(_storageFile.GetBlockBytes(StorageFile.HeaderBlock));
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes((uint)FirstFreeBlock);
        }
    }
}

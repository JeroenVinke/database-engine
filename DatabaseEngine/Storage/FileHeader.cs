using System;
using System.Drawing;
using System.Threading;

namespace DatabaseEngine
{
    public class FileHeader
    {
        private StorageFile _storageFile;

        public const int DefaultFirstBlock = 4;
        public int FirstFreeBlock { get; set; } = DefaultFirstBlock;

        public FileHeader(StorageFile storageFile)
        {
            _storageFile = storageFile;

            BlockBuffer buffer = Read();
            FirstFreeBlock = buffer.ReadByte();

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
            return BitConverter.GetBytes(FirstFreeBlock);
        }
    }
}

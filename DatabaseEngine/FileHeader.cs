using System;
using System.Drawing;
using System.Threading;

namespace DatabaseEngine
{
    public class FileHeader
    {
        private StorageFile _storageFile;

        public int FirstFreeBlock { get; set; } = 2;

        public FileHeader(StorageFile storageFile)
        {
            _storageFile = storageFile;

            BlockBuffer buffer = Read();
            FirstFreeBlock = buffer.ReadByte();
        }


        private BlockBuffer Read()
        {
            return _storageFile.GetBlockBytes(StorageFile.HeaderBlock);
        }
    }
}

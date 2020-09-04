using System;

namespace DatabaseEngine.Storage
{
    public class MemoryBuffer : IDisposable
    {
        private StorageFile _storageFile;
        private MemoryManager _memoryManager;
        private Block Block { get; set; }
        public bool Filled => Block != null;
        public uint? Page => Block?.Page.PageNumber;
        private object _lock = new object();
        public DateTime LastAccession { get; set; }
        public static int Reads = 0;

        public MemoryBuffer(MemoryManager memoryManager, StorageFile storageFile)
        {
            _storageFile = storageFile;
            _memoryManager = memoryManager;
        }

        public Block GetBlock()
        {
            LastAccession = DateTime.Now;
            return Block;
        }

        public Block Read(Relation relation, Pointer pointer)
        {
            lock (_lock)
            {
                if (Filled)
                {
                    throw new Exception("Memory buffer already filled");
                }

                Block = _storageFile.ReadBlock(_memoryManager, relation, pointer);
                Reads++;

                return GetBlock();
            }
        }

        public void Update(Block block)
        {
            Block = block;
        }

        public void Dispose()
        {
            lock(_lock)
            {
                Block = null;
            }
        }
    }
}

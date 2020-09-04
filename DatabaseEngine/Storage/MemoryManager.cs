using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace DatabaseEngine.Storage
{
    public class MemoryManager
    {
        // todo calculate max memory buffers
        public int M => 100; // 100 blocks
        private HashSet<uint> _cachedBlocks = new HashSet<uint>();
        private List<MemoryBuffer> _buffers;
        private StorageFile _storageFile;
        private int MaximumCacheDuration = 1;
        private Timer _cleanupTimer;
        public static int Writes = 0;

        public MemoryManager(StorageFile storageFile)
        {
            _buffers = new List<MemoryBuffer>(M);

            for(int i = 0; i < M; i++)
            {
                _buffers.Add(new MemoryBuffer(this, storageFile));
            }

            _storageFile = storageFile;
            _cleanupTimer = new Timer(30000);
            _cleanupTimer.Elapsed += CleanupTimer_Elapsed;
            _cleanupTimer.Start();
        }

        private void CleanupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Cleanup();
        }

        internal void Dispose()
        {
            _cleanupTimer.Stop();
            Cleanup();
        }

        public Pointer GetFreeBlock()
        {
            return _storageFile.GetFreeBlock();
        }

        public Block Read(Relation relation, Pointer page)
        {
            if (_cachedBlocks.Contains(page.PageNumber))
            {
                // cache hit
                return _buffers.First(x => x.Page == page.PageNumber).GetBlock();
            }

            MemoryBuffer freeBuffer = _buffers.FirstOrDefault(x => !x.Filled);

            if (freeBuffer != null)
            {
                _cachedBlocks.Add(page.PageNumber);
                // cache miss, load in cache
                return freeBuffer.Read(relation, page);
            }

            // cache miss, no free cache 
            return _storageFile.ReadBlock(this, relation, page);
        }

        public void Persist(Block block)
        {
            Writes++;

            _storageFile.WriteBlock(block);

            Pointer page = block.Page;
            if (_cachedBlocks.Contains(page.PageNumber))
            {
                // cache hit
                _buffers.First(x => x.Page == page.PageNumber).Update(block);
            }
        }

        public void Cleanup()
        {
            int cleaned = 0;
            int kept = 0;
            foreach(MemoryBuffer buffer in _buffers)
            {
                if (buffer.Filled && (DateTime.Now - buffer.LastAccession).TotalMinutes >= MaximumCacheDuration)
                {
                    _cachedBlocks.Remove((uint)buffer.Page);
                    buffer.Dispose();
                    cleaned++;
                }
                else if (buffer.Filled)
                {
                    kept++;
                }
            }

            if (Program.Debug)
            {
                Console.WriteLine("[DEBUG]: Cleanup (cleaned: " + cleaned + ", kept: " + kept + ", total free: " + (_buffers.Count - kept));
            }
        }
    }
}

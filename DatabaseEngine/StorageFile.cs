using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DatabaseEngine
{
    public class StorageFile
    {
        public FileHeader Header { get; set; }
        private IntPtr _fileHandle;
        private const int HeaderBlocks = 1;
        public const int HeaderBlock = 0;
        public const int RootBlock = 1;

        public StorageFile(string filePath)
        {
            _fileHandle = OpenOrCreateFile(filePath);
            Header = new FileHeader(this);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            uint lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            uint hTemplateFile
        );


        public delegate void WriteFileCompletionDelegate(UInt32 dwErrorCode,
          UInt32 dwNumberOfBytesTransfered, ref NativeOverlapped lpOverlapped);

        private IntPtr OpenOrCreateFile(string filePath)
        {
            IntPtr fileHandle = CreateFile(filePath,
                      (uint)NativeFileAccess.GENERIC_READ | (uint)NativeFileAccess.GENERIC_WRITE,
                      (uint)NativeShareMode.FILE_SHARE_READ | (uint)NativeShareMode.FILE_SHARE_WRITE,
                      0,
                      (uint)NativeCreationDeposition.OPEN_ALWAYS,
                      (uint)FileAttribute.NORMAL,
                      0);

            return fileHandle;
        }

        public BlockBuffer GetBlockBytes(int pageNumber)
        {
            NativeOverlapped overlapped = new NativeOverlapped();
            overlapped.OffsetLow = Block.BlockSize * pageNumber;
            byte[] buffer = new byte[Block.BlockSize];
            uint readBytes;
            Block.ReadFile(_fileHandle, buffer, (uint)Block.BlockSize, out readBytes, ref overlapped);

            return new BlockBuffer(buffer);
        }

        public Block ReadBlock(int pageNumber)
        {
            BlockBuffer buffer = GetBlockBytes(pageNumber + HeaderBlocks);

            return Block.CreateIndexBlockFromBuffer(buffer);
        }

        public Pointer GetFreeBlock()
        {
            // free block pointer bijhouden in file header
            return null;
        }

        public void Write(IntPtr fileHandle)
        {
            //byte[] blockBytes = ToBytes();

            //NativeOverlapped overlapped = new NativeOverlapped();
            //overlapped.OffsetLow = 0;
            //WriteFileEx(fileHandle, blockBytes, (uint)blockBytes.Length, ref overlapped, Completed);
        }

        private static void Completed(uint dwErrorCode, uint dwNumberOfBytesTransfered, ref NativeOverlapped lpOverlapped)
        {
            ;
        }
    }
}

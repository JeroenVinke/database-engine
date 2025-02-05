﻿using DatabaseEngine.Storage;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DatabaseEngine
{
    public class StorageFile : IDisposable
    {
        public FileHeader Header { get; set; }
        private IntPtr _fileHandle;
        public const uint HeaderBlock = 0;
        public const uint RootBlock = 1;

        public enum NativeFileAccess : uint
        {
            GENERIC_READ = 0x80000000u,
            GENERIC_WRITE = 0x40000000u
        }

        public enum NativeShareMode : uint
        {
            FILE_SHARE_READ = 0x1,
            FILE_SHARE_WRITE = 0x2u
        }

        public enum NativeCreationDeposition : uint
        {
            OPEN_EXISTING = 0x3u,
            OPEN_ALWAYS = 0x4u
        }

        public enum FileAttribute : uint
        {
            NORMAL = 0x80u
        }

        [DllImport("kernel32.dll")]
        public static extern bool WriteFileEx(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            [In] ref System.Threading.NativeOverlapped lpOverlapped,
            WriteFileCompletionDelegate lpCompletionRoutine);

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

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            ref NativeOverlapped lpOverlapped
        );

        public StorageFile(string filePath)
        {
            CompletedDelegate = new WriteFileCompletionDelegate(Completed);
            _fileHandle = OpenOrCreateFile(filePath);
            Header = new FileHeader(this);
        }

        public void WriteHeader()
        {
            WriteBlock(StorageFile.HeaderBlock, Header.ToBytes());
        }

        public void Dispose()
        {
            WriteHeader();
        }

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

        public byte[] GetBlockBytes(uint pageNumber)
        {
            NativeOverlapped overlapped = new NativeOverlapped();
            overlapped.OffsetLow = (int)(Block.BlockSize * pageNumber);
            byte[] buffer = new byte[Block.BlockSize];
            uint readBytes;
            ReadFile(_fileHandle, buffer, (uint)Block.BlockSize, out readBytes, ref overlapped);

            return buffer;
        }

        public Block ReadBlock(MemoryManager memoryManager, Relation relation, Pointer pageNumber)
        {
            byte[] buffer = GetBlockBytes(pageNumber.PageNumber);

            return new Block(memoryManager, relation, buffer, pageNumber);
        }

        private WriteFileCompletionDelegate CompletedDelegate;

        private void WriteBlock(uint pageNumber, byte[] content)
        {
            NativeOverlapped overlapped = new NativeOverlapped();
            overlapped.OffsetLow = (int)(pageNumber * Block.BlockSize);
            WriteFileEx(_fileHandle, content, (uint)content.Length, ref overlapped, CompletedDelegate);
        }

        public void WriteBlock(Block block)
        {
            WriteBlock(block.Page.PageNumber, block.ToBytes());
        }

        public Pointer GetFreeBlock()
        {
            Header.FirstFreeBlock++;
            // free block pointer bijhouden in file header
            return new Pointer(Header.FirstFreeBlock, 0);
        }

        private void Completed(uint dwErrorCode, uint dwNumberOfBytesTransfered, ref NativeOverlapped lpOverlapped)
        {
            ;
        }
    }
}

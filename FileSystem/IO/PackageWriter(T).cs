﻿using FileSystem.Components;
using FileSystem.Enums;
using System.Runtime.InteropServices;
using ZstdNet;

namespace FileSystem.IO
{
    public unsafe class PackageWriter<TMetadata> : IDisposable
        where TMetadata : struct
    {
        private readonly FileStream fileStream;
        private readonly Compressor zstdCompressor;

        private PackageHeader packageHeader;
        private long nextHeaderPointer;

        public PackageWriter(FileStream fileStream)
        {
            this.fileStream = fileStream;
            zstdCompressor = new(new(CompressionOptions.MaxCompressionLevel));

            packageHeader.FirstHeaderAddress = PackageHeader.Size;
            nextHeaderPointer = PackageHeader.HeaderAddressOffset;

            Write(ref packageHeader);
        }

        public void Write<T>(int index, ref T content, CompressionAlgorithm compression, TMetadata metadata = default)
            where T : struct
        {
            WriteSpan(index, MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref content, 1)), compression, metadata);
        }

        public void WriteSpan<T>(int index, Span<T> span, CompressionAlgorithm compression, TMetadata metadata = default)
            where T : struct
        {
            WriteSpan(index, MemoryMarshal.AsBytes(span), compression, metadata);
        }

        public void WriteSpan(int index, Span<byte> span, CompressionAlgorithm compression, TMetadata metadata = default)
        {
            if (span.Length == 0)
                return;

            packageHeader.FileCount++;
            
            FileHeader<TMetadata> header = new()
            {
                Index = index,
                UncompressedSize = span.Length,
                CompressionAlgorithm = compression,
                ContentHash = Utility.CalculateHash(span),
                Metadata = metadata
            };

            UpdateLastHeader();
            nextHeaderPointer = fileStream.Length + FileHeader<TMetadata>.HeaderAddressOffset;

            if (header.CompressionAlgorithm == CompressionAlgorithm.None)
            {
                header.CompressedSize = span.Length;
                Write(ref header);
                fileStream.Write(span);

                return;
            }

            if (header.CompressionAlgorithm == CompressionAlgorithm.Zstd)
            {
                byte[] compressedFile = zstdCompressor.Wrap(span);

                header.CompressedSize = compressedFile.Length;
                Write(ref header);
                fileStream.Write(compressedFile);

                return;
            }

            throw new NotSupportedException("Compression algorithm not supported");
        }

        private void Write<T>(ref T value) where T : struct
        {
            fileStream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)));
        }

        private void UpdateLastHeader()
        {
            fileStream.Seek(nextHeaderPointer, SeekOrigin.Begin);
            fileStream.Write(BitConverter.GetBytes((int)fileStream.Length));
            fileStream.Seek(0, SeekOrigin.End);
        }

        public void Dispose()
        {
            if (packageHeader.FileCount == 0)
                packageHeader.FirstHeaderAddress = 0;

            fileStream.Seek(0, SeekOrigin.Begin);
            Write(ref packageHeader);

            GC.SuppressFinalize(this);
        }
    }
}

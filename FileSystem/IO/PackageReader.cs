using FileSystem.Components;
using FileSystem.Enums;
using Microsoft.Win32.SafeHandles;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using ZstdNet;

namespace FileSystem.IO
{
    public sealed unsafe class PackageReader : IDisposable
    {
        private readonly MemoryMappedFile file;
        private readonly MemoryMappedViewAccessor accessor;
        private readonly SafeMemoryMappedViewHandle handle;

        private readonly Header[] headers;
        private readonly Decompressor zstdDecompressor;

        public PackageReader(FileStream fileStream)
        {
            file = MemoryMappedFile.CreateFromFile(fileStream, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
            accessor = file.CreateViewAccessor(0, fileStream.Length, MemoryMappedFileAccess.Read);
            handle = accessor.SafeMemoryMappedViewHandle;

            zstdDecompressor = new();
            PackageHeader packageHeader = handle.Read<PackageHeader>(0);

            List<HeaderPair> fileHeaders = new(packageHeader.FileCount);

            FileHeader currentHeader = new() { NextHeaderAddress = packageHeader.FirstHeaderAddress };
            int currentPosition = PackageHeader.Size;

            while (currentHeader.NextHeaderAddress != 0)
            {
                currentHeader = handle.Read<FileHeader>((ulong)currentHeader.NextHeaderAddress);
                fileHeaders.Add(new() { Header = currentHeader, ContentAddress = currentPosition + FileHeader.Size });
                currentPosition = currentHeader.NextHeaderAddress;
            }

            fileHeaders.Sort();

            Span<HeaderPair> span = CollectionsMarshal.AsSpan(fileHeaders);
            headers = new Header[span[^1].Header.Index + 1];

            for (int i = 0; i < span.Length; i++)
            {
                (FileHeader header, int contentAddress) = span[i];

                headers[header.Index] = new()
                {
                    ContentAddress = contentAddress,
                    CompressedSize = header.CompressedSize,
                    UncompressedSize = header.UncompressedSize,
                    CompressionAlgorithm = header.CompressionAlgorithm
                };
            }
        }

        public void ReadSpan<T>(int index, Span<T> span)
            where T : struct
        {
            ReadSpan(index, MemoryMarshal.AsBytes(span));
        }

        public void ReadSpan(int index, Span<byte> span)
        {
            Header header = headers[index];

            if (header.ContentAddress == 0)
                return;

            if (header.CompressionAlgorithm == CompressionAlgorithm.None)
            {
                handle.ReadSpan((ulong)header.ContentAddress, span[..header.UncompressedSize]);
                return;
            }

            if (header.CompressionAlgorithm == CompressionAlgorithm.Zstd)
            {
                IntPtr pointer = IntPtr.Add(handle.DangerousGetHandle(), header.ContentAddress);
                ReadOnlySpan<byte> compressed = new(pointer.ToPointer(), header.CompressedSize);

                zstdDecompressor.Unwrap(compressed, span, false);
                return;
            }

            throw new NotSupportedException("Compression algorithm not supported");
        }

        public Span<T> ReadSpan<T>(int index)
            where T : struct
        {
            Span<byte> span = ReadSpan(index);
            return MemoryMarshal.Cast<byte, T>(span);
        }

        public Span<byte> ReadSpan(int index)
        {
            Header header = headers[index];

            if (header.ContentAddress == 0)
                return Span<byte>.Empty;

            if (header.CompressionAlgorithm == CompressionAlgorithm.None)
            {
                Span<byte> buffer = new byte[header.CompressedSize];
                handle.ReadSpan((ulong)header.ContentAddress, buffer);

                return buffer;
            }

            if (header.CompressionAlgorithm == CompressionAlgorithm.Zstd)
            {
                IntPtr pointer = IntPtr.Add(handle.DangerousGetHandle(), header.ContentAddress);
                ReadOnlySpan<byte> compressed = new(pointer.ToPointer(), header.CompressedSize);

                Span<byte> uncompressed = new byte[header.UncompressedSize];

                zstdDecompressor.Unwrap(compressed, uncompressed, false);
                return uncompressed;
            }

            throw new NotSupportedException("Compression algorithm not supported");
        }

        public T Read<T>(int index) where T : struct
        {
            Header header = headers[index];

            if (header.ContentAddress == 0)
                return default;

            if (header.CompressionAlgorithm == CompressionAlgorithm.None)
                return handle.Read<T>((ulong)header.ContentAddress);

            if (header.CompressionAlgorithm == CompressionAlgorithm.Zstd)
            {
                IntPtr pointer = IntPtr.Add(handle.DangerousGetHandle(), header.ContentAddress);
                ReadOnlySpan<byte> compressed = new(pointer.ToPointer(), header.CompressedSize);

                Span<byte> uncompressed = header.UncompressedSize > 256 ? new byte[header.UncompressedSize] : stackalloc byte[header.UncompressedSize];

                zstdDecompressor.Unwrap(compressed, uncompressed, false);
                return MemoryMarshal.Read<T>(uncompressed);
            }

            throw new NotSupportedException("Compression algorithm not supported");
        }

        public void Dispose()
        {
            handle.Dispose();
            accessor.Dispose();
            file.Dispose();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Header
        {
            public CompressionAlgorithm CompressionAlgorithm;
            public int ContentAddress;
            public int CompressedSize;
            public int UncompressedSize;
        }

        private struct HeaderPair : IComparable<HeaderPair>
        {
            public FileHeader Header;
            public int ContentAddress;

            public int CompareTo(HeaderPair other)
            {
                return Header.CompareTo(other.Header);
            }

            public void Deconstruct(out FileHeader header, out int contentAddress)
            {
                header = Header;
                contentAddress = ContentAddress;
            }

            public static bool operator <(HeaderPair left, HeaderPair right)
            {
                return left.CompareTo(right) < 0;
            }

            public static bool operator >(HeaderPair left, HeaderPair right)
            {
                return left.CompareTo(right) > 0;
            }

            public static bool operator <=(HeaderPair left, HeaderPair right)
            {
                return left.CompareTo(right) <= 0;
            }

            public static bool operator >=(HeaderPair left, HeaderPair right)
            {
                return left.CompareTo(right) >= 0;
            }
        }
    }
}

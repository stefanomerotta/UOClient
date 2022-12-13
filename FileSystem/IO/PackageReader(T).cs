using FileSystem.Components;
using FileSystem.Enums;
using Microsoft.Win32.SafeHandles;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZstdNet;

namespace FileSystem.IO
{
    public unsafe class PackageReader<TMetadata> : IDisposable
        where TMetadata : struct
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

            FileHeader<TMetadata> currentHeader = new() { NextHeaderAddress = packageHeader.FirstHeaderAddress };
            int currentPosition = PackageHeader.Size;

            while (currentHeader.NextHeaderAddress != 0)
            {
                currentHeader = handle.Read<FileHeader<TMetadata>>((ulong)currentHeader.NextHeaderAddress);
                fileHeaders.Add(new(currentHeader, currentPosition + FileHeader<TMetadata>.Size));
                currentPosition = currentHeader.NextHeaderAddress;
            }

            fileHeaders.Sort();

            Span<HeaderPair> span = CollectionsMarshal.AsSpan(fileHeaders);
            headers = new Header[span[^1].Header.Index + 1];

            for (int i = 0; i < span.Length; i++)
            {
                (FileHeader<TMetadata> header, int contentAddress) = span[i];

                headers[header.Index] = new(contentAddress, in header);
            }
        }

        public ref readonly TMetadata GetMetadata(int index)
            => ref headers[index].Metadata;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadSpan<T>(int index, Span<T> span) where T : struct
            => ReadSpan(index, span, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadSpan(int index, Span<byte> span)
            => ReadSpan(index, span, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> ReadSpan<T>(int index) where T : struct
            => ReadSpan<T>(index, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadArray(int index)
            => ReadArray(index, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ReadArray<T>(int index) where T : struct
            => ReadArray<T>(index, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(int index) where T : struct
            => Read<T>(index, out _);

        public int ReadSpan<T>(int index, Span<T> span, out TMetadata metadata)
            where T : struct
        {
            return ReadSpan(index, MemoryMarshal.AsBytes(span), out metadata);
        }

        public int ReadSpan(int index, Span<byte> span, out TMetadata metadata)
        {
            Header header = headers[index];
            metadata = header.Metadata;

            if (header.ContentAddress == 0)
                return 0;

            if (header.CompressionAlgorithm == CompressionAlgorithm.None)
            {
                handle.ReadSpan((ulong)header.ContentAddress, span[..header.UncompressedSize]);
                return header.UncompressedSize;
            }

            if (header.CompressionAlgorithm == CompressionAlgorithm.Zstd)
            {
                IntPtr pointer = IntPtr.Add(handle.DangerousGetHandle(), header.ContentAddress);
                ReadOnlySpan<byte> compressed = new(pointer.ToPointer(), header.CompressedSize);

                zstdDecompressor.Unwrap(compressed, span, false);
                return header.UncompressedSize;
            }

            throw new NotSupportedException("Compression algorithm not supported");
        }

        public Span<T> ReadSpan<T>(int index, out TMetadata metadata)
            where T : struct
        {
            Span<byte> span = ReadArray(index, out metadata);
            return MemoryMarshal.Cast<byte, T>(span);
        }

        public byte[] ReadArray(int index, out TMetadata metadata)
        {
            Header header = headers[index];
            metadata = header.Metadata;

            if (header.ContentAddress == 0)
                return Array.Empty<byte>();

            if (header.CompressionAlgorithm == CompressionAlgorithm.None)
            {
                byte[] buffer = new byte[header.CompressedSize];
                handle.ReadSpan((ulong)header.ContentAddress, buffer.AsSpan());

                return buffer;
            }

            if (header.CompressionAlgorithm == CompressionAlgorithm.Zstd)
            {
                IntPtr pointer = IntPtr.Add(handle.DangerousGetHandle(), header.ContentAddress);
                ReadOnlySpan<byte> compressed = new(pointer.ToPointer(), header.CompressedSize);

                byte[] uncompressed = new byte[header.UncompressedSize];

                zstdDecompressor.Unwrap(compressed, uncompressed, false);
                return uncompressed;
            }

            throw new NotSupportedException("Compression algorithm not supported");
        }

        public T[] ReadArray<T>(int index, out TMetadata metadata)
            where T : struct
        {
            Header header = headers[index];
            metadata = header.Metadata;

            if (header.ContentAddress == 0)
                return Array.Empty<T>();

            if (header.CompressionAlgorithm == CompressionAlgorithm.None)
            {
                T[] buffer = new T[header.CompressedSize];
                handle.ReadSpan((ulong)header.ContentAddress, buffer.AsSpan());

                return buffer;
            }

            if (header.CompressionAlgorithm == CompressionAlgorithm.Zstd)
            {
                IntPtr pointer = IntPtr.Add(handle.DangerousGetHandle(), header.ContentAddress);
                ReadOnlySpan<byte> compressed = new(pointer.ToPointer(), header.CompressedSize);

                T[] uncompressed = new T[header.UncompressedSize / Unsafe.SizeOf<T>()];

                zstdDecompressor.Unwrap(compressed, MemoryMarshal.AsBytes(uncompressed.AsSpan()), false);
                return uncompressed;
            }

            throw new NotSupportedException("Compression algorithm not supported");
        }

        public T Read<T>(int index, out TMetadata metadata) where T : struct
        {
            Header header = headers[index];
            metadata = header.Metadata;

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

            GC.SuppressFinalize(this);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private readonly struct Header
        {
            public readonly CompressionAlgorithm CompressionAlgorithm;
            public readonly int ContentAddress;
            public readonly int CompressedSize;
            public readonly int UncompressedSize;
            public readonly TMetadata Metadata;

            public Header(int contentAddress, in FileHeader<TMetadata> header)
            {
                ContentAddress = contentAddress;
                CompressedSize = header.CompressedSize;
                UncompressedSize = header.UncompressedSize;
                CompressionAlgorithm = header.CompressionAlgorithm;
                Metadata = header.Metadata;
            }
        }

        private readonly struct HeaderPair : IComparable<HeaderPair>
        {
            public readonly FileHeader<TMetadata> Header;
            public readonly int ContentAddress;

            public HeaderPair(FileHeader<TMetadata> header, int contentAddress)
            {
                Header = header;
                ContentAddress = contentAddress;
            }

            public int CompareTo(HeaderPair other)
            {
                return Header.CompareTo(other.Header);
            }

            public void Deconstruct(out FileHeader<TMetadata> header, out int contentAddress)
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

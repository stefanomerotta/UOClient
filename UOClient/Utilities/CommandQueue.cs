using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace UOClient.Utilities
{
    internal sealed class CommandQueue<T> : IDisposable
    {
        private readonly Channel<T> channel;
        private readonly ChannelReader<T> reader;
        private readonly ChannelWriter<T> writer;

        public CommandQueue(int capacity)
        {
            channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait
            });

            reader = channel.Reader;
            writer = channel.Writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue([NotNullWhen(true)] out T? command)
        {
            return reader.TryRead(out command!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<T> DequeueAsync(CancellationToken token = default)
        {
            return reader.ReadAsync(token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(T command)
        {
            return writer.TryWrite(command);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask EnqueueAsync(T command, CancellationToken token = default)
        {
            return writer.WriteAsync(command, token);
        }

        public void Dispose()
        {
            writer.Complete();
        }
    }
}

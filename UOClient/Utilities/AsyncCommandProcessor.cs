using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace UOClient.Utilities
{
    internal sealed class AsyncCommandProcessor<T> : IAsyncDisposable
    {
        private readonly CommandQueue<T> queue;
        private readonly Func<T, ValueTask> asyncWorker;
        private readonly Task asyncWorkerTask;

        public AsyncCommandProcessor(int capacity, Func<T, ValueTask> worker, CancellationToken token = default)
        {
            queue = new(capacity);
            asyncWorker = worker;
            asyncWorkerTask = DoAsyncWork(token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(T command)
        {
            return queue.TryEnqueue(command);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask EnqueueAsync(T command, CancellationToken token = default)
        {
            return queue.EnqueueAsync(command, token);
        }

        private async Task DoAsyncWork(CancellationToken token)
        {
            await Task.Yield();

            while (!token.IsCancellationRequested)
            {
                T command = await queue.DequeueAsync();
                await asyncWorker.Invoke(command);
            }
        }

        public async ValueTask DisposeAsync()
        {
            queue.Dispose();
            await asyncWorkerTask;
        }
    }
}

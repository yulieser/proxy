using CRProxy.Applications;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace CRProxy.Infrastructure.Backgrounds.QueueServices
{
    public class BackgroundTaskQueueService : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<(HandleClientDelegate callback, CancellationToken cancellationToken, TcpClient client)> 
            _processItemsQueue;
        private readonly SemaphoreSlim _signalSemaphoreSlim;

        public BackgroundTaskQueueService()
        {
            _processItemsQueue = new ConcurrentQueue<(HandleClientDelegate callback, CancellationToken cancellationToken, 
                TcpClient client)>();
            _signalSemaphoreSlim = new SemaphoreSlim(0);
        }
        public async Task<(HandleClientDelegate callback, CancellationToken cancellationToken, TcpClient client)>
            DequeueAsync(CancellationToken cancellationToken)
        {
            await _signalSemaphoreSlim.WaitAsync(cancellationToken);
            _processItemsQueue.TryDequeue(out var processItem);

            return processItem;
        }

        public Task QueueBackgroundWorkItemAsync((HandleClientDelegate callback, CancellationToken cancellationToken, TcpClient client) 
            processItem)
        {
            _processItemsQueue.Enqueue(processItem);
            _signalSemaphoreSlim.Release();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _processItemsQueue.Clear();
            _signalSemaphoreSlim.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}

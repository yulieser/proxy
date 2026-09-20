using System.Net.Sockets;

namespace CRProxy.Applications
{
    public interface IBackgroundTaskQueue
    {
        Task QueueBackgroundWorkItemAsync((HandleClientDelegate callback, CancellationToken cancellationToken, TcpClient client) 
            processItem);
        Task<(HandleClientDelegate callback, CancellationToken cancellationToken, TcpClient client)> DequeueAsync(
            CancellationToken cancellationToken);
        void Dispose();
    }
}

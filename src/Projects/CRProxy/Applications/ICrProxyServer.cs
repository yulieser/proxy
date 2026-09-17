namespace CRProxy.Applications
{
    public interface ICrProxyServer
    {
        Task StartServerAsync(CancellationToken cancellationToken, IServiceProvider serviceProvider);
        void Stop();
    }
}

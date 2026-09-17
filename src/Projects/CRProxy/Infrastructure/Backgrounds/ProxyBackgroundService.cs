using CRProxy.Applications;

namespace CRProxy.Infrastructure.Backgrounds
{
    public class ProxyBackgroundService(IServiceProvider serviceProvider) : IHostedService
    {
        private IServiceProvider ServiceProvider { get; } = serviceProvider;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = StartServerAsync(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task StartServerAsync(CancellationToken cancellationToken) 
        {
            using var scope = ServiceProvider.CreateScope();
            var proxyServer = scope.ServiceProvider.GetRequiredService<ICrProxyServer>();
            await proxyServer.StartServerAsync(cancellationToken, scope.ServiceProvider);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

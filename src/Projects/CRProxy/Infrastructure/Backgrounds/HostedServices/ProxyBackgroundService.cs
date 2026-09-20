using CRProxy.Applications;

namespace CRProxy.Infrastructure.Backgrounds.HostedServices
{
    public class ProxyBackgroundService(IServiceProvider serviceProvider) : IHostedService
    {
        private IServiceProvider ServiceProvider { get; } = serviceProvider;

        // Cancellation source used to request shutdown of the long-running server task
        private CancellationTokenSource? _linkedCts;
        // The background server task
        private Task? _serverTask;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a linked token so we can request cancellation independently while honoring host shutdown
            _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _serverTask = StartServerAsync(_linkedCts.Token);
            return Task.CompletedTask;
        }

        private async Task StartServerAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var proxyServer = scope.ServiceProvider.GetRequiredService<ICrProxyServer>();
                await proxyServer.StartServerAsync(cancellationToken, scope.ServiceProvider);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // expected during shutdown - swallow to allow graceful exit
            }
            catch (Exception)
            {
                // Intentionally swallow other exceptions here to avoid crashing the host from background work.
                // Consider logging or bubbling up depending on desired behavior.
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_linkedCts == null)
                return;

            try
            {
                // Request the server task to stop
                _linkedCts.Cancel();

                if (_serverTask != null)
                {
                    // Wait for either the server task to complete or the host cancellation to trigger
                    var completed = await Task.WhenAny(_serverTask, Task.Delay(Timeout.Infinite, cancellationToken));
                    // If the completed task is the server task, await it to observe exceptions
                    if (completed == _serverTask)
                        await _serverTask; // propagate exception if any (will be caught below)
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Stop was aborted by host token - nothing more to do
            }
            catch (Exception)
            {
                // Swallow exceptions during shutdown; consider logging for observability
            }
            finally
            {
                _linkedCts.Dispose();
                _linkedCts = null;
                _serverTask = null;
            }
        }
    }
}

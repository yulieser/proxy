using CRProxy.Applications;
using CRProxy.Infrastructure.Backgrounds.HostedServices;
using CRProxy.Infrastructure.Backgrounds.QueueServices;
using CRProxy.Infrastructure.Services;

namespace CRProxy.Infrastructure
{
    public static class RegisterIoC
    {
        public static void SetupIoCContainer(IConfiguration Configuration, IServiceCollection services)
        {
            lock (services)
            {
                services.AddSingleton<IProxyConfiguration, CoreProxyConfiguration>(x =>
                {
                    return Configuration.GetSection("ProxyConfigServer").Get<CoreProxyConfiguration>()
                        ?? new CoreProxyConfiguration();
                });

                // Connection manager used by the proxy server
                services.AddSingleton<ConnectionManager>();

                // Add basic observability: counters for active connections and errors
                services.AddSingleton<Observability.ProxyMetrics>();
                services.AddSingleton<ICrProxyServer, CrProxyServer>();
                services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueueService>();
            }
        }

        public static void SetupHostedService(IServiceCollection services)
        {
            lock (services)
            {
                services.AddHostedService<ProxyBackgroundService>();
                services.AddHostedService<ManageConnectionBackgroundService>();
            }
        }
    }
}

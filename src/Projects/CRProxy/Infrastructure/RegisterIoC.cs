using CRProxy.Applications;
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

                services.AddSingleton<ICrProxyServer, CrProxyServer>();
            }
        }

        public static void SetupHostedService(IServiceCollection services)
        {
            lock (services)
            {
                services.AddHostedService<Backgrounds.ProxyBackgroundService>();
            }
        }
    }
}

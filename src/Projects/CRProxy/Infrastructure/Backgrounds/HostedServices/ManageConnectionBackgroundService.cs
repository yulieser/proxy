using CRProxy.Applications;

namespace CRProxy.Infrastructure.Backgrounds.HostedServices
{
    public class ManageConnectionBackgroundService(IBackgroundTaskQueue taskQueue) : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue = taskQueue;

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // It create the background hosted for process that will run.
            while (!cancellationToken.IsCancellationRequested)
            {
                var processItem = await _taskQueue.DequeueAsync(cancellationToken);

                try
                {
                    _ = processItem.callback(processItem.client, processItem.cancellationToken);
                }
                catch (Exception ex)
                {
                    // TODO: Consider logging the exception or handling it appropriately.
                }
            }
        }

        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _taskQueue.Dispose();
        }
    }
}

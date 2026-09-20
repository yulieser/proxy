using System.Threading;

namespace CRProxy.Infrastructure.Observability
{
    // Simple in-memory metrics for demonstration purposes.
    public class ProxyMetrics
    {
        private int _activeConnections;
        private int _errors;

        public int ActiveConnections => Volatile.Read(ref _activeConnections);
        public int Errors => Volatile.Read(ref _errors);

        public void IncrementActiveConnections() => Interlocked.Increment(ref _activeConnections);
        public void DecrementActiveConnections() => Interlocked.Decrement(ref _activeConnections);
        public void IncrementErrors() => Interlocked.Increment(ref _errors);
    }
}

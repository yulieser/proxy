using System.Text;

namespace CRProxy.Applications
{
    public interface IProxyConfiguration
    {
        bool IsEnabled { get; }
        int Port { get; }
        int Timeout { get; }
        string? Username { get; }
        string? Password { get; }
        string? Encoding { get; }
        Encoding GetEncoding { get; }
    }
}

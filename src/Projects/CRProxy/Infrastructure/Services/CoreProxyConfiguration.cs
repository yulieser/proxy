using CRProxy.Applications;
using System.Text;

namespace CRProxy.Infrastructure.Services
{
    public class CoreProxyConfiguration : IProxyConfiguration
    {
        public CoreProxyConfiguration() 
        {
            Port = 8080;
            Timeout = 60000;
            Username= "admin";
            Password= "admin";
            Encoding = "UTF-8";
            IsEnabled = false;
        }

        public int Port { get; set; }
        public int Timeout { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        public string? Encoding { get; set; }

        public Encoding GetEncoding
        {
            get
            {
                if (Encoding != null)
                {
                    try
                    {
                        return System.Text.Encoding.GetEncoding(Encoding);
                    }
                    catch (ArgumentException)
                    {
                        // Fallback to ASCII if the configured encoding is not supported
                        return System.Text.Encoding.UTF8;
                    }
                    catch (Exception)
                    {
                        // For any other unexpected error, fallback to ASCII
                        return System.Text.Encoding.UTF8;
                    }
                }

                return System.Text.Encoding.UTF8;
            }
        }

        public bool IsEnabled { get; set; }
    }
}

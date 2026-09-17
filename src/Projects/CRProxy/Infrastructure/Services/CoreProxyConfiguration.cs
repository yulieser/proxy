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
            Encoding = "ASCII";
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
                if(Encoding != null)
                {
                    return System.Text.Encoding.GetEncoding(Encoding);
                }
                return System.Text.Encoding.ASCII;
            }
        }

        public bool IsEnabled { get; set; }
    }
}

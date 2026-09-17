namespace CRES.CRProxy.Core.Helpers
{
    public class ConstantHelper
    {
        public static readonly string HeaderAuthScheme = "Basic";
        public static readonly string OkAccepted = "HTTP/1.1 200 Connection established\r\n\r\n";
        public static readonly string ProxyAuthenticationRequiredMessage = "Proxy Authentication Required";
        public static readonly string ForbiddenMessage = "Forbbiden";
        public static readonly string HttpScheme = "HTTP/1.1";
        public static readonly string ProxyAgent = "Proxy-agent";
        public static readonly string ProxyAgentValue = "CR-Proxy/1.1";
        public static readonly string LastLine = "\r\n\r\n";
        public static readonly string TimeoutMessage = "Connection timeout";
        public static readonly string UnavailableMessage = "Unreached server";
        public static readonly string ConnectionTimeoutHeader = "Connection-Timeout";
        public static readonly string ConnectCommand = "CONNECT";
    }
}

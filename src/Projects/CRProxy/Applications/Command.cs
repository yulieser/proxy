using CRES.CRProxy.Core.Helpers;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace CRProxy.Applications
{
    public class Command
    {
        internal static string GetTimeoutCommand(TimeSpan timeout)
        {
            return GetResponseCommandToClient(StatusCodes.Status408RequestTimeout, ConstantHelper.TimeoutMessage,
                timeout.TotalSeconds.ToString(), AddTimeoutCustomHeaders);
        }

        internal static string GetUnAuthorizedCommand()
        {
            return GetResponseCommandToClient(StatusCodes.Status401Unauthorized, ConstantHelper.ForbiddenMessage,
                $"{ConstantHelper.HeaderAuthScheme.Trim()} realm=\"(user:pass)\"", AddUnAuthorizedCustomHeaders);
        }

        internal static string GetProxyAuthenticationRequiredCommand()
        {
            return GetResponseCommandToClient(StatusCodes.Status407ProxyAuthenticationRequired,
                ConstantHelper.ProxyAuthenticationRequiredMessage,
                $"{ConstantHelper.HeaderAuthScheme.Trim()} realm=\"(user:pass)\"", AddProxyAuthenticateCustomHeaders);
        }

        internal static string GetServiceUnavailableCommand()
        {
            return GetResponseCommandToClient(StatusCodes.Status503ServiceUnavailable, ConstantHelper.UnavailableMessage, null, null);
        }

        private static void AddTimeoutCustomHeaders(StringBuilder commandBuilderResponse, string headerValue)
        {
            commandBuilderResponse.Append($"{ConstantHelper.ConnectionTimeoutHeader}: {headerValue}");
            commandBuilderResponse.Append(Environment.NewLine);
            commandBuilderResponse.Append($"{HeaderNames.Connection}: {ConstantHelper.ConnectionTimeoutHeader}");
            commandBuilderResponse.Append(Environment.NewLine);
        }

        private static void AddUnAuthorizedCustomHeaders(StringBuilder commandBuilderResponse, string headerValue)
        {
            commandBuilderResponse.Append($"{HeaderNames.WWWAuthenticate}: {headerValue}");
            commandBuilderResponse.Append(Environment.NewLine);
        }

        private static void AddProxyAuthenticateCustomHeaders(StringBuilder commandBuilderResponse, string headerValue)
        {
            commandBuilderResponse.Append($"{HeaderNames.ProxyAuthenticate}: {headerValue}");
            commandBuilderResponse.Append(Environment.NewLine);
        }

        private static string GetResponseCommandToClient(int code, string description, string? headerValue,
            AddPairValueHeaderDelegate? invokeCustomMessage)
        {
            var builder = new StringBuilder();
            builder.Append($"{ConstantHelper.HttpScheme} {code} {description}");
            builder.Append(Environment.NewLine);

            invokeCustomMessage?.Invoke(builder, headerValue ?? string.Empty);

            builder.Append($"{ConstantHelper.ProxyAgent}: {ConstantHelper.ProxyAgentValue}");
            builder.Append(Environment.NewLine);
            builder.Append($"{HeaderNames.Date}: {DateTime.Now.ToString()}");
            builder.Append(ConstantHelper.LastLine);

            return builder.ToString();
        }
    }
}

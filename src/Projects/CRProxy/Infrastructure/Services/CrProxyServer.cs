using CRES.CRProxy.Core.Helpers;
using CRProxy.Applications;
using CRProxy.Applications.Extensions;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CRProxy.Infrastructure.Services
{
    public class CrProxyServer : ICrProxyServer
    {
        public CrProxyServer(IProxyConfiguration proxyConfiguration)
        {
            ProxyConfiguration = proxyConfiguration;
            TcpServerListener = new TcpListener(IPAddress.Any, ProxyConfiguration.Port);
            ConnectionManager = new ConnectionManager();
            //Logger = null;
        }

        private IProxyConfiguration ProxyConfiguration { get; }
        //private MessageLogger? Logger { get; set; }
        private TcpListener TcpServerListener { get; }
        private ConnectionManager ConnectionManager { get; }

        public async Task StartServerAsync(CancellationToken cancellationToken, IServiceProvider serviceProvider)
        {
            // We can't start the server if it's not active.
            if (!ProxyConfiguration.IsEnabled)
            {
                return;
            }

            InitLogger(serviceProvider);
            StartTrace();
            TcpServerListener.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient? client = await TcpServerListener.AcceptTcpClientAsync(cancellationToken);
                await HandleClientAsync(client, cancellationToken);
            }
        }

        private void InitLogger(IServiceProvider serviceProvider) 
        {
            /*var loggerFactory = serviceProvider.GetRequiredService<ILoggersFactory>();
            var configurationRulesVariable = serviceProvider.GetRequiredService<IConfigurationSystemRulesVariable>();
            Logger = loggerFactory.GetMsgLogger(LoggerType.ProxyDaemonProcess, configurationRulesVariable);*/

        }

        private void ClientTrace(TcpClient client, string command)
        {
            /*var data = new AdditionalInfoLogData();
            data.Add("Info", "Accepting incoming client.");
            if(client?.Client?.RemoteEndPoint != null)
            {
                data.Add("Client IP", ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
                data.Add("Client Port", ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString());
                data.Add("Command", $"{Environment.NewLine}{command}");
            }
            Logger?.Trace(new GeneralContextObject(data));*/
        }

        private void StartTrace() 
        {
            /*var data = new AdditionalInfoLogData();
            data.Add("Info", "Starting daemon proxy server.");
            data.Add("Listened at Local IP", GetLocalIPAddress());
            data.Add("Listened at Port", ProxyConfiguration.Port.ToString());
            
            Logger?.Trace(new GeneralContextObject(data));*/
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var clientConnection = ConnectionManager.CreateNewConnection(client, ProxyConfiguration.Timeout);
            var thereIsResponse = await clientConnection.WaitForDataAvailableWithTimeoutAsync();
            if (thereIsResponse)
            {
                await ProcessClientStreamAsync(clientConnection, client.ReceiveBufferSize, cancellationToken);
            }
        }

        private async Task ProcessClientStreamAsync(Connection clientConnection, int bufferSize, CancellationToken cancellationToken)
        {
            var bytes = new byte[bufferSize];

            if (clientConnection.CurrentStream.DataAvailable)
            {
                var count = await clientConnection.CurrentStream.ReadAsync(bytes, cancellationToken);
                var command = bytes.ReadDataCommand(count, ProxyConfiguration.GetEncoding);

                // Trace.
                ClientTrace(clientConnection.Client, command);

                if (!IsHeaderProxyAuthorizationPresent(command))
                {
                    var commandResponse = Command.GetProxyAuthenticationRequiredCommand();
                    await clientConnection.CurrentStream.SendCommandToClientAsync(commandResponse, cancellationToken);
                }
                else
                {
                    if (!IsAuthorizedToUseProxyServer(command))
                    {
                        var commandResponse = Command.GetUnAuthorizedCommand();
                        await clientConnection.CurrentStream.SendCommandToClientAsync(commandResponse, cancellationToken);
                    }
                    else
                    {
                        await EstablishCommunicationWithServerAndExchangeDataAsync(command, clientConnection, cancellationToken);
                    }
                }
            }
        }

        private async Task EstablishCommunicationWithServerAndExchangeDataAsync(string command, Connection clientConnection,
            CancellationToken cancellationToken)
        {
            var encodedHost = command.GetHeaderValue(HeaderNames.Host);
            var (serverUrl, port) = encodedHost.GetServerEndpoint();
            var serverConnection = ConnectionManager.CreateNewConnection(ProxyConfiguration.Timeout);

            await serverConnection.ConnectAsync(serverUrl, port);
            if (serverConnection.Client.Connected)
            {
                if (command.IsAnSpecificHttpCommand(ConstantHelper.ConnectCommand))
                {
                    await clientConnection.CurrentStream.WriteAsync(Encoding.UTF8.GetBytes(ConstantHelper.OkAccepted), cancellationToken);
                    await clientConnection.CurrentStream.FlushAsync(cancellationToken);

                    var stopWatch = Stopwatch.StartNew();
                    var thereIsResponse = await clientConnection.WaitForDataAvailableWithTimeoutAsync();
                    stopWatch.Stop();

                    if (thereIsResponse)
                    {
                        ConnectionManager.StartExchangeBetweenConnectionsHttpsProtocol(clientConnection, serverConnection, cancellationToken);
                    }
                    else
                    {
                        // Timeout error.
                        var commandResponse = Command.GetTimeoutCommand(stopWatch.Elapsed);
                        await clientConnection.CurrentStream.SendCommandToClientAsync(commandResponse, cancellationToken);
                    }
                }
                else
                {
                    command = command.RemoveHeaderAndItsValue(HeaderNames.ProxyAuthorization);
                    await ConnectionManager.StartExchangeBetweenConnectionsHttpProtocolAsync(clientConnection, serverConnection, command, 
                        ProxyConfiguration.GetEncoding, cancellationToken);
                }
            }
            else
            {
                // The connection fail so I need to send back the info. with the correct description.
                var commandResponse = Command.GetServiceUnavailableCommand();
                await clientConnection.CurrentStream.SendCommandToClientAsync(commandResponse, cancellationToken);
            }
        }

        private static bool IsHeaderProxyAuthorizationPresent(string command)
        {
            return command.HasSpecificHeader(HeaderNames.ProxyAuthorization);
        }

        private bool IsAuthorizedToUseProxyServer(string command)
        {
            var encodedAuth = command.GetHeaderValue(HeaderNames.ProxyAuthorization).Replace(ConstantHelper.HeaderAuthScheme, string.Empty)
                .Trim();
            var (username, password) = encodedAuth.DecodeUserIdAndPassword(ProxyConfiguration.GetEncoding);

            return username == ProxyConfiguration.Username && password == ProxyConfiguration.Password;
        }

        public void Stop()
        {
            TcpServerListener.Stop();
        }
    }
}

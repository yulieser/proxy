using CRProxy.Applications.Extensions;
using System.Diagnostics;
using System.Net.Sockets;

namespace CRProxy.Applications
{
    public class Connection : IDisposable
    {
        private NetworkStream? _currentStream;
        private bool _active;

        public Connection(TcpClient client)
        {
            Client = client;
            Stopwatch = Stopwatch.StartNew();
            _currentStream = client.Connected ? client.GetStream() : null;
            LimitInactiveTime = 5000;
            TotalCycle = 4;
            _active = true;
        }

        public TcpClient Client { get; }
        public int InactivityTime { get; set; }
        private Stopwatch Stopwatch { get; set; }
        private Task? SendEventTask { get; set; }
        public NetworkStream CurrentStream
        {
            get
            {
                if (_currentStream == null)
                {
                    if (Client.Connected)
                    {
                        _currentStream = Client.GetStream();
                    }
                }

                return _currentStream;
            }
        }
        private int LimitInactiveTime { get; set; }
        private int TotalCycle { get; set; }
        public bool IsActive
        {
            get
            {
                // It's reached total inactive time so the cycle will be broken.
                if (Stopwatch.Elapsed.TotalMilliseconds >= LimitInactiveTime * 2)
                {
                    SignAsDisposable();
                }
                return _active;
            }
            private set => _active = value;
        }

        public async Task<bool> WaitForDataAvailableWithTimeoutAsync()
        {
            if (!IsActive)
            {
                return false;
            }
            return await CurrentStream.WaitForDataAvailableWithTimeoutAsync(Client.ReceiveTimeout);
        }

        public Task StartToExchangeDataAsync(NetworkStream destination, CancellationToken cancellationToken)
        {
            SendEventTask = SendDataAsync(destination, cancellationToken);
            return SendEventTask;
        }

        public Task StartToExchangeDataAsync(byte[] buffer, NetworkStream destination, CancellationToken cancellationToken)
        {
            SendEventTask = SendDataAsync(buffer, destination, cancellationToken);
            return SendEventTask;
        }

        public async Task ConnectAsync(string url, int port)
        {
            if (!Client.Connected)
            {
                await Client.ConnectAsync(url, port);
            }
        }

        private async Task SendDataAsync(byte[] buffer, NetworkStream destination, CancellationToken cancellationToken)
        {
            try
            {
                Stopwatch.Restart();
                IsActive = true;

                await buffer.WriteBufferToDestinationAsync(destination, cancellationToken);

            }
            catch (Exception)
            {
                // The connection was closed.
            }
        }

        private async Task SendDataAsync(NetworkStream destination, CancellationToken cancellationToken)
        {
            try
            {
                Stopwatch.Restart();
                IsActive = true;

                while (true)
                {
                    // It's reached total inactive time so the cycle will be broken.
                    if (Stopwatch.Elapsed.TotalMilliseconds >= LimitInactiveTime)
                    {
                        break;
                    }

                    if (CurrentStream.DataAvailable)
                    {
                        var totalRead = await CurrentStream.WriteStreamToDestinationAsync(destination, cancellationToken);
                        if (totalRead == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        var res = await CurrentStream.WaitForDataAvailableWithTimeoutAsync(timeout: null, TotalCycle);
                        if (res)
                        {
                            Stopwatch.Restart();
                        }
                    }
                }
                IsActive = false;
            }
            catch (Exception)
            {
                // The connection was closed.
                IsActive = false;
            }
        }

        private void SignAsDisposable()
        {
            IsActive = false;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (CurrentStream != null)
            {
                CurrentStream.Socket.Shutdown(SocketShutdown.Both);
                CurrentStream.Socket.Disconnect(false);
                CurrentStream.Socket.Close();
                CurrentStream.Socket.Dispose();

                CurrentStream.Close();
                CurrentStream.Dispose();
            }

            Client.Close();
            Client.Dispose();
        }
    }
}

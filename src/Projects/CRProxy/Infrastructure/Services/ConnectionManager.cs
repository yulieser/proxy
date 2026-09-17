using CRProxy.Applications;
using System.Net.Sockets;
using System.Text;

namespace CRProxy.Infrastructure.Services
{
    public class ConnectionManager
    {
        public ConnectionManager() 
        {
            Connections = [];
            Tasks = [];

            _ = CleanAsync();
        }

        private List<Connection> Connections { get; set; }
        private List<Task> Tasks { get; set; }

        public Connection CreateNewConnection(TcpClient client, int timeout)
        {
            client.ReceiveTimeout = timeout;
            client.SendTimeout = timeout;

            var res = new Connection(client);
            
            lock (Connections)
            {
                Connections.Add(res);
            }
            return res;
        }

        public Connection CreateNewConnection(int timeout) 
        {
            var client = new TcpClient
            {
                LingerState = new LingerOption(false, 0),
                ReceiveTimeout = timeout,
                SendTimeout = timeout,
                Client = new Socket(SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = timeout,
                    SendTimeout = timeout,
                    LingerState = new LingerOption(false, 0),
                    Blocking = true,
                }
            };

            var res = new Connection(client);

            lock (Connections)
            {
                Connections.Add(res);
            }
            return res;
        }

        public void StartExchangeBetweenConnectionsHttpsProtocol(Connection clientConnection, Connection serverConnection, 
            CancellationToken cancellation)
        {
            // Sending data to server.
            var taskSender = clientConnection.StartToExchangeDataAsync(serverConnection.CurrentStream, cancellation);
            // Receiving from server.
            var taskReceiver = serverConnection.StartToExchangeDataAsync(clientConnection.CurrentStream, cancellation);

            Tasks.Add(taskSender);
            Tasks.Add(taskReceiver);
        }

        public static async Task StartExchangeBetweenConnectionsHttpProtocolAsync(Connection clientConnection, Connection serverConnection,
            string command, Encoding encoding, CancellationToken cancellation)
        {
            var buffer = encoding.GetBytes(command);
            
            // Sending data to server.
            await clientConnection.StartToExchangeDataAsync(buffer, serverConnection.CurrentStream, cancellation);
            // Receiving from server.
            await serverConnection.StartToExchangeDataAsync(clientConnection.CurrentStream, cancellation);
        }

        public async Task CleanAsync() 
        {
            while (true)
            {
                lock (Connections)
                {
                    CleanConnections();
                    CleanTasks();
                }
                await Task.Delay(200);
            }
            
            
        }

        private void CleanConnections() 
        {
            if (Connections.Count == 0)
            {
                return;
            }
            var toRemove = Connections.Where(c => !c.IsActive).ToList();
            Connections = [.. Connections.Where(c => c.IsActive)];
            toRemove.ForEach(c =>
            {
                c.Dispose();
            });
        }

        private void CleanTasks() 
        {
            if(Tasks.Count == 0)
            {
                return;
            }
            var toRemove = Tasks.Where(t => t.IsCompleted || t.IsCanceled || t.IsFaulted || t.IsCompletedSuccessfully);
            Tasks = [.. Tasks.Where(t => !(t.IsCompleted || t.IsCanceled || t.IsFaulted || t.IsCompletedSuccessfully))];
            foreach (var item in toRemove)
            {
                item.Dispose();
            }
        }
    }
}

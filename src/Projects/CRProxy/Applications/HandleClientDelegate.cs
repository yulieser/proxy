using System.Net.Sockets;

namespace CRProxy.Applications;
public delegate Task HandleClientDelegate(TcpClient client, CancellationToken cancellationToken);

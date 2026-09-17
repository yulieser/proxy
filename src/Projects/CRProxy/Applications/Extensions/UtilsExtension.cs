using System.Net.Sockets;
using System.Text;

namespace CRProxy.Applications.Extensions
{
    public static class UtilsExtension
    {
        internal static string ReadDataCommand(this byte[] buffer, int countRead, Encoding encoding)
        {
            return encoding.GetString(buffer, 0, countRead);
        }

        internal static bool HasSpecificHeader(this string command, string header)
        {
            return command.Contains(header);
        }

        internal static bool IsAnSpecificHttpCommand(this string command, string httpCommand)
        {
            return command.Trim().StartsWith(httpCommand, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static (string username, string password) DecodeUserIdAndPassword(this string encodedAuth, Encoding encoding)
        {
            var userpass = encoding.GetString(Convert.FromBase64String(encodedAuth));
            var separator = userpass.IndexOf(':');
            if (separator == -1)
                return (string.Empty, string.Empty);

            return (userpass[..separator], userpass[(separator + 1)..]);
        }

        internal static async Task<int> WriteStreamToDestinationAsync(this NetworkStream origin, NetworkStream destination,
            CancellationToken cancellationToken)
        {
            var totalRead = 0;
            if (origin != null && origin.CanRead && destination != null && destination.CanWrite)
            {
                var buffer = new byte[1024];
                while (origin.DataAvailable)
                {
                    var count = await origin.ReadAsync(buffer, cancellationToken);
                    if (count > 0)
                    {
                        totalRead += count;
                        await destination.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
                    }
                }
                await destination.FlushAsync(cancellationToken);
            }
            return totalRead;
        }

        internal static async Task<int> WriteBufferToDestinationAsync(this byte[] origin, NetworkStream destination,
            CancellationToken cancellationToken)
        {
            var totalRead = 0;
            if (destination != null && destination.CanWrite)
            {
                totalRead += origin.Length;
                await destination.WriteAsync(origin.AsMemory(0, origin.Length), cancellationToken);
                await destination.FlushAsync(cancellationToken);
            }
            return totalRead;
        }

        internal static (string serverUrl, int port) GetServerEndpoint(this string encodedHost)
        {
            if (!string.IsNullOrEmpty(encodedHost))
            {
                var splitItems = encodedHost.Split(":", StringSplitOptions.RemoveEmptyEntries);
                if (splitItems.Length == 2)
                {
                    return (serverUrl: splitItems[0].Trim(), port: int.Parse(splitItems[1].Trim()));
                }
                else
                {
                    return (serverUrl: splitItems[0].Trim(), port: 80);
                }
            }

            return (string.Empty, -1);
        }

        internal static string RemoveHeaderAndItsValue(this string command, string header)
        {
            if (string.IsNullOrEmpty(command))
                return command;

            var key = $"{header}:";
            var start = command.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (start < 0)
            {
                return command;
            }

            // Find end of line sequence after the header
            var searchPos = start + key.Length;
            var endSeq = command.IndexOf("\r\n", searchPos, StringComparison.Ordinal);
            int end;
            int seqLength;
            if (endSeq >= 0)
            {
                end = endSeq;
                seqLength = 2; // \r\n
            }
            else
            {
                endSeq = command.IndexOf('\n', searchPos);
                if (endSeq >= 0)
                {
                    end = endSeq;
                    seqLength = 1; // \n only
                }
                else
                {
                    // No EOL found; remove until end of string
                    end = command.Length;
                    seqLength = 0;
                }
            }

            var lengthToRemove = (end + seqLength) - start;
            if (lengthToRemove <= 0)
                return command;

            return command.Remove(start, lengthToRemove);
        }

        internal static string GetHeaderValue(this string command, string header)
        {
            if (string.IsNullOrEmpty(command))
                return string.Empty;

            var key = $"{header}:";
            var index = command.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return string.Empty;

            index += key.Length;

            // Find end of line for the header value
            var endSeq = command.IndexOf("\r\n", index, StringComparison.Ordinal);
            int end;
            if (endSeq >= 0)
            {
                end = endSeq;
            }
            else
            {
                endSeq = command.IndexOf('\n', index);
                end = endSeq >= 0 ? endSeq : command.Length;
            }

            // Trim the extracted value
            var value = command.Substring(index, end - index);
            return value.Trim();
        }

        private static bool IsScapeCharacter(char c)
        {
            return c == '\r' || c == '\n';
        }
    }
}

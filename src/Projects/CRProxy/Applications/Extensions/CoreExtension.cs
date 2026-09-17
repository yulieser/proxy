using System.Net.Sockets;
using System.Text;

namespace CRProxy.Applications.Extensions
{
    public static class CoreExtension
    {
        internal static async Task SendCommandToClientAsync(this NetworkStream clientStream, string command,
            CancellationToken cancellationToken)
        {
            await clientStream.WriteAsync(Encoding.UTF8.GetBytes(command), cancellationToken);
            await clientStream.FlushAsync(cancellationToken);
        }

        internal static async Task<bool> WaitForDataAvailableWithTimeoutAsync(this NetworkStream from, int? timeout = null,
            int? totalCycle = null)
        {
            if (timeout == null && totalCycle == null)
            {
                ArgumentNullException argumentNullException = new(message: "It's required timeout or totalCycle parameters.",
                    innerException: null);
                throw argumentNullException;
            }

            var _countCycle = 0;
            var _timeWaiting = 500;
            var response = true;

            while (!from.DataAvailable)
            {
                if (timeout != null)
                {
                    if (_countCycle * _timeWaiting > timeout)
                    {
                        response = false;
                        break;
                    }
                }
                else
                {
                    if (totalCycle != null && totalCycle <= _countCycle)
                    {
                        response = false;
                        break;
                    }
                }

                await Task.Delay(_timeWaiting);
                _countCycle++;
            }

            return response;
        }
    }
}

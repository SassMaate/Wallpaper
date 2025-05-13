using System.Net;
using System.Net.Sockets;

namespace Sucrose.Transmission.Helper
{
    internal class TransmissionClient : IDisposable
    {
        private bool _isConnected;
        private StreamWriter _writer;
        private TcpClient _tcpClient;

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public async Task Start(IPAddress host, int port)
        {
            _tcpClient = new();

            await _tcpClient.ConnectAsync(host, port);

            _isConnected = true;

            NetworkStream stream = _tcpClient.GetStream();

            _writer = new StreamWriter(stream)
            {
                AutoFlush = true
            };
        }

        public async Task Stop()
        {
            _isConnected = false;

            if (_writer != null)
            {
#if NET9_0
                await _writer.DisposeAsync();
#else
                _writer.Dispose();
#endif

                _writer = null;
            }

            if (_tcpClient != null)
            {
                if (_tcpClient.Connected)
                {
                    _tcpClient.Close();
                }

#if NET9_0
                await Task.Run(() => _tcpClient.Dispose());
#else
                _tcpClient.Dispose();
#endif

                _tcpClient = null;
            }
        }

        public async Task SendMessage(string Message)
        {
            if (_tcpClient == null || !_isConnected || !IsConnected)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(Message))
            {
                await _writer.WriteLineAsync(Message);
            }
        }

        public void Dispose()
        {
            _ = Stop();
        }
    }
}
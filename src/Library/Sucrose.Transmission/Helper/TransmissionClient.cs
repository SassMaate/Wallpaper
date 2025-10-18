using System.Net;
using System.Net.Sockets;

namespace Sucrose.Transmission.Helper
{
    internal class TransmissionClient : IDisposable
    {
        private bool _isConnected;
        private TcpClient _tcpClient;
        private StreamWriter _writer;
        private SemaphoreSlim _sendSemaphore = new(1, 1);

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public async Task Start(IPAddress host, int port)
        {
            try
            {
                // Ensure clean state
                if (_tcpClient != null)
                {
                    await Stop();
                }

                _tcpClient = new TcpClient();

                // Set timeout for connection (5 seconds)
                using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

                await _tcpClient.ConnectAsync(host, port, cts.Token);

                if (_tcpClient.Connected)
                {
                    _isConnected = true;

                    // Configure keep-alive
                    _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    _tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 10);
                    _tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 2);

                    // Set receive and send timeouts
                    _tcpClient.SendTimeout = 2500; // 2.5 seconds
                    _tcpClient.ReceiveTimeout = 5000; // 5 seconds

                    NetworkStream stream = _tcpClient.GetStream();

                    _writer = new StreamWriter(stream)
                    {
                        AutoFlush = true
                    };
                }
                else
                {
                    throw new InvalidOperationException("Failed to establish connection");
                }
            }
            catch (OperationCanceledException)
            {
                _isConnected = false;

                throw new TimeoutException($"Connection to {host}:{port} timed out");
            }
            catch (SocketException Exception)
            {
                _isConnected = false;

                await Stop();

                throw new InvalidOperationException($"Failed to connect to {host}:{port}: {Exception.Message}", Exception);
            }
            catch (Exception)
            {
                _isConnected = false;

                await Stop();

                throw;
            }
        }

        public async Task Stop()
        {
            _isConnected = false;

            try
            {
                if (_writer != null)
                {
                    await _writer.DisposeAsync();

                    _writer = null;
                }
            }
            catch { }

            try
            {
                if (_tcpClient != null)
                {
                    if (_tcpClient.Connected)
                    {
                        _tcpClient.Close();
                    }

                    _tcpClient.Dispose();

                    _tcpClient = null;
                }
            }
            catch { }
        }

        public async Task SendMessage(string Message)
        {
            if (_tcpClient == null || !_isConnected || !IsConnected)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(Message))
            {
                await _sendSemaphore.WaitAsync();

                try
                {
                    await _writer.WriteLineAsync(Message);
                }
                catch (IOException Exception)
                {
                    _isConnected = false;

                    throw new InvalidOperationException("Failed to send message. Connection may be lost.", Exception);
                }
                catch (ObjectDisposedException)
                {
                    _isConnected = false;

                    throw new InvalidOperationException("Connection is closed.");
                }
                finally
                {
                    _sendSemaphore.Release();
                }
            }
        }

        public void Dispose()
        {
            _ = Stop();

            _sendSemaphore?.Dispose();

            _sendSemaphore = new(1, 1);
        }
    }
}
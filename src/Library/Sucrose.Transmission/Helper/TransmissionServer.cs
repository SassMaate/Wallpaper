using System.Net;
using System.Net.Sockets;
using STEMREA = Sucrose.Transmission.Event.MessageReceivedEventArgs;

namespace Sucrose.Transmission.Helper
{
    internal class TransmissionServer : IDisposable
    {
        private bool _isRunning;
        private TcpClient _client;
        private StreamReader _reader;
        private TcpListener _tcpListener;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsConnected => _client?.Connected ?? false;

        public async Task Start(IPAddress host, int port, EventHandler<STEMREA> eventHandler)
        {
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            _tcpListener = new TcpListener(host, port);
            _tcpListener.Start();

            while (_isRunning)
            {
                try
                {
                    _client = await _tcpListener.AcceptTcpClientAsync();
                    NetworkStream stream = _client.GetStream();
                    _reader = new StreamReader(stream);

                    while (_isRunning && _client.Connected)
                    {
                        try
                        {
                            string message = await _reader.ReadLineAsync();

                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                eventHandler?.Invoke(this, new STEMREA { Message = message });
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }

                    if (_reader != null)
                    {
                        _reader.Dispose();
                        _reader = null;
                    }

                    if (_client != null)
                    {
                        _client.Close();
                        _client.Dispose();
                        _client = null;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
        }

        public async Task Stop()
        {
            _isRunning = false;

            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                await _cancellationTokenSource.CancelAsync();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }

            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
            }

            if (_tcpListener != null)
            {
                _tcpListener.Stop();

                await Task.CompletedTask;
            }
        }

        public void Dispose()
        {
            _ = Stop();
        }
    }
}
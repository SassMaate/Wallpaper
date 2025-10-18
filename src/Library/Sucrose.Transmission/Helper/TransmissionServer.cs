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
        private SemaphoreSlim _clientLock = new(1, 1);
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsConnected => _client?.Connected ?? false;

        public async Task Start(IPAddress host, int port, EventHandler<STEMREA> eventHandler)
        {
            // Ensure clean state
            if (_tcpListener != null)
            {
                await Stop();
            }

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            _tcpListener = new TcpListener(host, port);
            _tcpListener.Start();

            while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // Accept client with cancellation support
                    using CancellationTokenSource acceptCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);
                    acceptCts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 second timeout for accepting connections

                    Task<TcpClient> tcpClientTask = _tcpListener.AcceptTcpClientAsync();
                    TaskCompletionSource<bool> acceptTcs = new();

                    using (acceptCts.Token.Register(() => acceptTcs.TrySetCanceled()))
                    {
                        Task completedTask = await Task.WhenAny(tcpClientTask, acceptTcs.Task);

                        if (completedTask == acceptTcs.Task)
                        {
                            continue; // Timeout or cancellation
                        }

                        _client = await tcpClientTask;
                    }

                    // Configure the accepted client
                    ConfigureClient(_client);

                    NetworkStream stream = _client.GetStream();
                    _reader = new StreamReader(stream);

                    while (_isRunning && _client.Connected && !_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            // Read with timeout
                            using CancellationTokenSource readCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);
                            readCts.CancelAfter(TimeSpan.FromSeconds(15)); // 15 second read timeout

                            Task<string> readTask = _reader.ReadLineAsync();
                            TaskCompletionSource<string> readTcs = new();

                            using (readCts.Token.Register(() => readTcs.TrySetCanceled()))
                            {
                                Task<string> completedTask = await Task.WhenAny(readTask, readTcs.Task);

                                if (completedTask == readTcs.Task)
                                {
                                    break; // Timeout or cancellation
                                }

                                string message = await readTask;
                                if (message == null) // End of stream
                                {
                                    break;
                                }

                                if (!string.IsNullOrWhiteSpace(message))
                                {
                                    eventHandler?.Invoke(this, new STEMREA { Message = message });
                                }
                            }
                        }
                        catch (IOException)
                        {
                            // Connection lost
                            break;
                        }
                        catch (ObjectDisposedException)
                        {
                            // Reader disposed
                            break;
                        }
                        catch (OperationCanceledException)
                        {
                            // Cancellation requested
                            break;
                        }
                    }

                    // Clean up client connection
                    await CleanupClient();
                }
                catch (OperationCanceledException)
                {
                    // Server shutdown requested
                    break;
                }
                catch (SocketException)
                {
                    // Socket error, wait before retry
                    if (_isRunning)
                    {
                        await Task.Delay(1000, _cancellationTokenSource.Token).ConfigureAwait(false);
                    }
                }
                catch (Exception)
                {
                    // Unexpected error, wait before retry
                    if (_isRunning)
                    {
                        await Task.Delay(1000, _cancellationTokenSource.Token).ConfigureAwait(false);
                    }
                }
            }
        }

        public async Task Stop()
        {
            _isRunning = false;

            try
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    await _cancellationTokenSource.CancelAsync();
                }
            }
            catch { }

            await CleanupClient();

            try
            {
                if (_tcpListener != null)
                {
                    _tcpListener.Stop();

                    _tcpListener = null;
                }
            }
            catch { }

            try
            {
                _cancellationTokenSource?.Dispose();

                _cancellationTokenSource = null;
            }
            catch { }
        }

        private void ConfigureClient(TcpClient client)
        {
            if (client != null && client.Connected)
            {
                // Configure keep-alive
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 10);
                client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 2);

                // Set timeouts
                client.SendTimeout = 5000; // 5 seconds
                client.ReceiveTimeout = 15000; // 15 seconds
            }
        }

        private async Task CleanupClient()
        {
            await _clientLock.WaitAsync();

            try
            {
                if (_reader != null)
                {
                    try
                    {
                        _reader.Dispose();
                    }
                    catch { }

                    _reader = null;
                }

                if (_client != null)
                {
                    try
                    {
                        if (_client.Connected)
                        {
                            _client.Close();
                        }

                        _client.Dispose();
                    }
                    catch { }

                    _client = null;
                }
            }
            finally
            {
                _clientLock.Release();
            }
        }

        public void Dispose()
        {
            _ = Stop();

            _clientLock?.Dispose();

            _clientLock = new(1, 1);
        }
    }
}
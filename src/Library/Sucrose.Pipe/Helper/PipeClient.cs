using System.IO.Pipes;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;

namespace Sucrose.Pipe.Helper
{
    internal class PipeClient : IDisposable
    {
        private bool _isConnected;
        private StreamWriter _writer;
        private NamedPipeClientStream _pipeClient;
        private SemaphoreSlim _sendSemaphore = new(1, 1);

        public bool IsConnected => _pipeClient?.IsConnected ?? false;

        public async Task Start(string pipeName)
        {
            try
            {
                // Ensure clean state
                if (_pipeClient != null)
                {
                    await Stop();
                }

                _pipeClient = new(SMMRG.PipeServerName, pipeName, PipeDirection.Out, PipeOptions.Asynchronous);

                // Set timeout for connection (5 seconds)
                using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

                await _pipeClient.ConnectAsync(cts.Token);

                if (_pipeClient.IsConnected)
                {
                    _isConnected = true;

                    _writer = new(_pipeClient)
                    {
                        AutoFlush = true
                    };
                }
                else
                {
                    throw new InvalidOperationException("Failed to establish pipe connection");
                }
            }
            catch (OperationCanceledException)
            {
                _isConnected = false;

                throw new TimeoutException($"Connection to pipe '{pipeName}' timed out");
            }
            catch (IOException Exception)
            {
                _isConnected = false;

                await Stop();

                throw new InvalidOperationException($"Failed to connect to pipe '{pipeName}': {Exception.Message}", Exception);
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
                if (_pipeClient != null)
                {
                    if (_pipeClient.IsConnected)
                    {
                        _pipeClient.Close();
                    }

                    await _pipeClient.DisposeAsync();

                    _pipeClient = null;
                }
            }
            catch { }
        }

        public async Task SendMessage(string message)
        {
            if (_pipeClient == null || !_isConnected || !IsConnected)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                await _sendSemaphore.WaitAsync();

                try
                {
                    await _writer.WriteLineAsync(message);
                }
                catch (IOException Exception)
                {
                    _isConnected = false;

                    throw new InvalidOperationException("Failed to send message. Pipe connection may be broken.", Exception);
                }
                catch (ObjectDisposedException)
                {
                    _isConnected = false;

                    throw new InvalidOperationException("Pipe connection is closed.");
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
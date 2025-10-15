using System.IO.Pipes;
using SPEMREA = Sucrose.Pipe.Event.MessageReceivedEventArgs;

namespace Sucrose.Pipe.Helper
{
    internal class PipeServer : IDisposable
    {
        private bool _isRunning;
        private StreamReader _reader;
        private NamedPipeServerStream _pipeServer;
        private readonly SemaphoreSlim _clientLock = new(1, 1);
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsConnected => _pipeServer?.IsConnected ?? false;

        public async Task Start(string pipeName, EventHandler<SPEMREA> eventHandler)
        {
            // Ensure clean state
            if (_pipeServer != null)
            {
                await Stop();
            }

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _pipeServer = new(pipeName, PipeDirection.In, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // Wait for connection with cancellation support
                    using CancellationTokenSource waitCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);
                    waitCts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 second timeout for accepting connections

                    await _pipeServer.WaitForConnectionAsync(waitCts.Token);
                    _reader = new(_pipeServer);

                    while (_isRunning && IsConnected && !_cancellationTokenSource.Token.IsCancellationRequested)
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
                                    eventHandler?.Invoke(this, new SPEMREA { Message = message });
                                }
                            }
                        }
                        catch (IOException)
                        {
                            // Pipe broken
                            break;
                        }
                        catch (ObjectDisposedException)
                        {
                            // Reader disposed
                            break;
                        }
                    }

                    // Clean up after client disconnects
                    await CleanupClient();
                }
                catch (OperationCanceledException)
                {
                    // Server shutdown requested
                    break;
                }
                catch (IOException)
                {
                    // Pipe error, recreate if still running
                    if (_isRunning)
                    {
                        await CleanupAndRecreate(pipeName);

                        await Task.Delay(1000, _cancellationTokenSource.Token).ConfigureAwait(false);
                    }
                }
                catch (Exception)
                {
                    // Unexpected error, recreate if still running
                    if (_isRunning)
                    {
                        await CleanupAndRecreate(pipeName);

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
                if (_pipeServer != null)
                {
                    if (_pipeServer.IsConnected)
                    {
                        _pipeServer.Disconnect();
                    }

                    await _pipeServer.DisposeAsync();

                    _pipeServer = null;
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

                if (_pipeServer != null && _pipeServer.IsConnected)
                {
                    try
                    {
                        _pipeServer.Disconnect();
                    }
                    catch { }
                }
            }
            finally
            {
                _clientLock.Release();
            }
        }

        private async Task CleanupAndRecreate(string pipeName)
        {
            await CleanupClient();

            try
            {
                if (_pipeServer != null)
                {
                    await _pipeServer.DisposeAsync();
                }
            }
            catch { }

            _pipeServer = new(pipeName, PipeDirection.In, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        public void Dispose()
        {
            _ = Stop();

            _clientLock?.Dispose();
        }
    }
}
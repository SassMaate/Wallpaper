using System.IO.Pipes;
using SPEMREA = Sucrose.Pipe.Event.MessageReceivedEventArgs;

namespace Sucrose.Pipe.Helper
{
    internal class PipeServer : IDisposable
    {
        private bool _isRunning;
        private StreamReader _reader;
        private NamedPipeServerStream _pipeServer;

        public bool IsConnected => _pipeServer?.IsConnected ?? false;

        public async Task Start(string pipeName, EventHandler<SPEMREA> eventHandler)
        {
            _isRunning = true;
            _pipeServer = new(pipeName, PipeDirection.In, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            while (_isRunning)
            {
                try
                {
                    await _pipeServer.WaitForConnectionAsync();
                    _reader = new(_pipeServer);

                    while (IsConnected)
                    {
                        string message = await _reader.ReadLineAsync();

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            eventHandler?.Invoke(this, new SPEMREA { Message = message });
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    if (_pipeServer != null)
                    {
                        if (_pipeServer.IsConnected)
                        {
                            _pipeServer.Disconnect();
                        }

                        _pipeServer.Dispose();
                        _pipeServer = new(pipeName, PipeDirection.In, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    }
                }
            }
        }

        public async Task Stop()
        {
            _isRunning = false;

            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }

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

        public void Dispose()
        {
            _ = Stop();
        }
    }
}
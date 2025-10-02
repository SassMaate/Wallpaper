using System.IO.Pipes;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;

namespace Sucrose.Pipe.Helper
{
    internal class PipeClient : IDisposable
    {
        private bool _isConnected;
        private StreamWriter _writer;
        private NamedPipeClientStream _pipeClient;

        public bool IsConnected => _pipeClient?.IsConnected ?? false;

        public async Task Start(string pipeName)
        {
            _pipeClient = new(SMMRG.PipeServerName, pipeName, PipeDirection.Out, PipeOptions.Asynchronous);

            await _pipeClient.ConnectAsync();
            _isConnected = true;

            _writer = new(_pipeClient)
            {
                AutoFlush = true
            };
        }

        public async Task Stop()
        {
            _isConnected = false;

            if (_writer != null)
            {
                await _writer.DisposeAsync();

                _writer = null;
            }

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

        public async Task SendMessage(string message)
        {
            if (_pipeClient == null || !_isConnected || !IsConnected)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                await _writer.WriteLineAsync(message);
            }
        }

        public void Dispose()
        {
            _ = Stop();
        }
    }
}
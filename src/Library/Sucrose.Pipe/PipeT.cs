using SPEMREA = Sucrose.Pipe.Event.MessageReceivedEventArgs;
using SPHPC = Sucrose.Pipe.Helper.PipeClient;
using SPHPS = Sucrose.Pipe.Helper.PipeServer;

namespace Sucrose.Pipe
{
    public class PipeT(string PipeName)
    {
        private readonly SPHPC PC = new();
        private readonly SPHPS PS = new();

        public event EventHandler<SPEMREA> MessageReceived;

        public async Task StartClient()
        {
            if (!PC.IsConnected)
            {
                try
                {
                    await PC.Stop();
                }
                catch { }

                await PC.Start(PipeName);
            }
        }

        public async Task StartClient(string Message)
        {
            if (!PC.IsConnected)
            {
                try
                {
                    await PC.Stop();
                }
                catch { }

                await PC.Start(PipeName);
            }

            await PC.SendMessage(Message);
        }

        public async Task StartServer()
        {
            if (!PS.IsConnected)
            {
                try
                {
                    await PS.Stop();
                }
                catch { }

                await PS.Start(PipeName, MessageReceived);
            }
        }

        public async Task StopClient()
        {
            await PC.Stop();
        }

        public async Task StopServer()
        {
            await PS.Stop();
        }

        public async Task DisposeClient()
        {
            await PC.Stop();

            PC.Dispose();
        }

        public async Task DisposeServer()
        {
            await PS.Stop();

            PS.Dispose();
        }

        protected virtual void OnMessageReceived(SPEMREA e)
        {
            MessageReceived?.Invoke(this, e);
        }
    }
}
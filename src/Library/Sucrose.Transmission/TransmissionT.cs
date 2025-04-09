using System.Net;
using STEMREA = Sucrose.Transmission.Event.MessageReceivedEventArgs;
using STHTC = Sucrose.Transmission.Helper.TransmissionClient;
using STHTS = Sucrose.Transmission.Helper.TransmissionServer;

namespace Sucrose.Transmission
{
    public class TransmissionT(IPAddress Host, int Port)
    {
        private readonly STHTC TC = new();
        private readonly STHTS TS = new();

        public int Port { get; private set; } = Port;

        public event EventHandler<STEMREA> MessageReceived;

        public async Task StartClient()
        {
            if (!TC.IsConnected)
            {
                try
                {
                    await TC.Stop();
                }
                catch { }

                await TC.Start(Host, Port);
            }
        }

        public async Task StartClient(string Message)
        {
            if (!TC.IsConnected)
            {
                try
                {
                    await TC.Stop();
                }
                catch { }

                await TC.Start(Host, Port);
            }

            await TC.SendMessage(Message);
        }

        public async Task StartServer()
        {
            if (!TS.IsConnected)
            {
                try
                {
                    await TS.Stop();
                }
                catch { }

                await TS.Start(Host, Port, MessageReceived);
            }
        }

        public async Task StopClient()
        {
            await TC.Stop();
        }

        public async Task StopServer()
        {
            await TS.Stop();
        }

        public async Task DisposeClient()
        {
            await TC.Stop();

            TC.Dispose();
        }

        public async Task DisposeServer()
        {
            await TS.Stop();

            TS.Dispose();
        }

        protected virtual void OnMessageReceived(STEMREA e)
        {
            MessageReceived?.Invoke(this, e);
        }
    }
}
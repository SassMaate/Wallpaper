using System.Net;
using System.Net.Sockets;
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
                await StartClientWithRetry();
            }
        }

        public async Task StartClient(string Message)
        {
            if (!TC.IsConnected)
            {
                await StartClientWithRetry();
            }

            if (TC.IsConnected)
            {
                try
                {
                    await TC.SendMessage(Message);
                }
                catch (InvalidOperationException Exception)
                {
                    // Connection lost, try to reconnect once
                    await StartClientWithRetry(maxRetries: 1);

                    if (TC.IsConnected)
                    {
                        await TC.SendMessage(Message);
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to send message after reconnection attempt", Exception);
                    }
                }
            }
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

        private async Task StartClientWithRetry(int maxRetries = 3)
        {
            Exception lastException = null;

            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    // Clean up previous connection
                    try
                    {
                        await TC.Stop();
                    }
                    catch { }

                    // Attempt to connect
                    await TC.Start(Host, Port);

                    // If successful, return
                    if (TC.IsConnected)
                    {
                        return;
                    }
                }
                catch (TimeoutException Exception)
                {
                    lastException = Exception;

                    if (i < maxRetries)
                    {
                        // Wait before retry with exponential backoff
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                    }
                }
                catch (InvalidOperationException Exception) when (Exception.InnerException is SocketException)
                {
                    lastException = Exception;

                    if (i < maxRetries)
                    {
                        // Wait before retry with exponential backoff
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                    }
                }
                catch (Exception Exception)
                {
                    lastException = Exception;

                    break; // Don't retry on unexpected exceptions
                }
            }

            // If we get here, all retries failed
            if (lastException != null)
            {
                throw new InvalidOperationException($"Failed to connect after {maxRetries} retries", lastException);
            }
        }
    }
}
using System.IO.Pipes;
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
                await StartClientWithRetry();
            }
        }

        public async Task StartClient(string Message)
        {
            if (!PC.IsConnected)
            {
                await StartClientWithRetry();
            }

            if (PC.IsConnected)
            {
                try
                {
                    await PC.SendMessage(Message);
                }
                catch (InvalidOperationException ex)
                {
                    // Connection lost, try to reconnect once
                    await StartClientWithRetry(maxRetries: 1);
                    
                    if (PC.IsConnected)
                    {
                        await PC.SendMessage(Message);
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to send message after reconnection attempt", ex);
                    }
                }
            }
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
                        await PC.Stop();
                    }
                    catch { }

                    // Attempt to connect
                    await PC.Start(PipeName);
                    
                    // If successful, return
                    if (PC.IsConnected)
                    {
                        return;
                    }
                }
                catch (TimeoutException ex)
                {
                    lastException = ex;
                    if (i < maxRetries)
                    {
                        // Wait before retry with exponential backoff
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                    }
                }
                catch (InvalidOperationException ex) when (ex.InnerException is IOException)
                {
                    lastException = ex;
                    if (i < maxRetries)
                    {
                        // Wait before retry with exponential backoff
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    break; // Don't retry on unexpected exceptions
                }
            }

            // If we get here, all retries failed
            if (lastException != null)
            {
                throw new InvalidOperationException($"Failed to connect to pipe after {maxRetries} retries", lastException);
            }
        }
    }
}
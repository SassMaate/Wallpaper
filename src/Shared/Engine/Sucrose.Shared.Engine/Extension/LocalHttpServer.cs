using System.IO;
using System.Net;
using SEET = Skylark.Enum.EncodeType;
using SHE = Skylark.Helper.Encode;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SSSHF = Sucrose.Shared.Space.Helper.Filing;
using SSSHP = Sucrose.Shared.Space.Helper.Port;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Shared.Engine.Extension
{
    internal class LocalHttpServer(string themeFolder, string customFolder = null)
    {
        private readonly int Port = SSSHP.Available(SMMRG.Loopback);
        private CancellationTokenSource _cancellationTokenSource;
        private readonly HttpListener Listener = new();
        private Task _listenerTask;

        public void Stop()
        {
            // Synchronous wrapper for backward compatibility
            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch { }
        }

        public string Host()
        {
            return $"http://{SMMRG.Loopback}:{Port}/";
        }

        public async Task StopAsync()
        {
            try
            {
                // Signal cancellation
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }
            }
            catch { }

            try
            {
                // Stop listener
                if (Listener != null && Listener.IsListening)
                {
                    Listener.Stop();
                    Listener.Close();
                }
            }
            catch { }

            try
            {
                // Wait for listener task to complete (with timeout)
                if (_listenerTask != null && !_listenerTask.IsCompleted)
                {
                    await _listenerTask.WaitAsync(TimeSpan.FromSeconds(2));
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

        public async Task StartAsync()
        {
            try
            {
                // Stop existing listener if any
                await StopAsync();

                Listener.Prefixes.Add(Host());
                Listener.Prefixes.Add($"http://localhost:{Port}/");

                if (string.IsNullOrEmpty(customFolder))
                {
                    customFolder = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.Content);
                }

                if (Listener != null && !Listener.IsListening)
                {
                    _cancellationTokenSource = new CancellationTokenSource();

                    Listener.Start();

                    // Start listener loop without blocking
                    _listenerTask = Task.Run(async () =>
                    {
                        try
                        {
                            await ListenerLoop(_cancellationTokenSource.Token);
                        }
                        catch (Exception Exception)
                        {
                            // Log any unhandled exceptions from the listener loop
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    }, _cancellationTokenSource.Token);
                }
            }
            catch (HttpListenerException Exception)
            {
                // Port might be in use or listener configuration error
                // Swallow exception to prevent application crash
                await SSWEW.Watch_CatchException(Exception);
            }
            catch (Exception Exception)
            {
                // Unexpected error during startup
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        private string GetContentType(string filename)
        {
            return Path.GetExtension(filename).ToLowerInvariant() switch
            {
                ".rar" => "application/x-rar-compressed",
                ".js" => "application/javascript",
                ".hdr" => "image/vnd.radiance",
                ".tar" => "application/x-tar",
                ".json" => "application/json",
                ".glb" => "model/gltf-binary",
                ".gltf" => "model/gltf+json",
                ".zip" => "application/zip",
                ".xml" => "application/xml",
                ".svg" => "image/svg+xml",
                ".woff2" => "font/woff2",
                ".md" => "text/markdown",
                ".ico" => "image/x-icon",
                ".webp" => "image/webp",
                ".webm" => "video/webm",
                ".tiff" => "image/tiff",
                ".jpeg" => "image/jpeg",
                ".woff" => "font/woff",
                ".txt" => "text/plain",
                ".jpg" => "image/jpeg",
                ".mp3" => "audio/mpeg",
                ".html" => "text/html",
                ".wav" => "audio/wav",
                ".png" => "image/png",
                ".ogg" => "audio/ogg",
                ".mp4" => "video/mp4",
                ".htm" => "text/html",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".ttf" => "font/ttf",
                ".otf" => "font/otf",
                ".csv" => "text/csv",
                ".css" => "text/css",
                _ => "application/octet-stream",
            };
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            if (context == null)
            {
                return;
            }

            HttpListenerResponse response = context.Response;

            try
            {
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS, PATCH");

                string themePath = Path.Combine(themeFolder, context.Request.Url.LocalPath.TrimStart('/'));
                string customPath = Path.Combine(customFolder, context.Request.Url.LocalPath.TrimStart('/'));

                if (File.Exists(themePath) || File.Exists(customPath))
                {
                    string path = string.Empty;

                    if (File.Exists(themePath))
                    {
                        path = themePath;
                    }
                    else
                    {
                        path = customPath;
                    }

                    await WriteFile(context, path);

                    //string filename = Path.GetFileName(path);
                    //byte[] content = File.ReadAllBytes(path);

                    //response.ContentLength64 = content.Length;
                    //response.StatusCode = (int)HttpStatusCode.NotModified;
                    //response.ContentType = GetContentType(path);

                    //await response.OutputStream.WriteAsync(content.AsMemory(0, content.Length));
                }
                else
                {
                    byte[] message = SHE.GetBytes("File not found", SEET.UTF8);

                    response.StatusCode = (int)HttpStatusCode.NotFound;

                    await response.OutputStream.WriteAsync(message.AsMemory(0, message.Length));
                }
            }
            catch (HttpListenerException)
            {
                // Client disconnected or response error
            }
            catch (ObjectDisposedException)
            {
                // Response was disposed
            }
            catch (Exception)
            {
                // Unexpected error
            }
            finally
            {
                try
                {
                    response.OutputStream.Close();
                    response.Close();
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }

        private async Task ListenerLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && Listener.IsListening)
            {
                try
                {
                    // GetContextAsync with cancellation support
                    Task<HttpListenerContext> getContextTask = Listener.GetContextAsync();
                    TaskCompletionSource<bool> tcs = new();

                    using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                    {
                        Task completedTask = await Task.WhenAny(getContextTask, tcs.Task);

                        if (completedTask == tcs.Task)
                        {
                            // Cancellation requested
                            break;
                        }

                        HttpListenerContext context = await getContextTask;

                        // Handle request in parallel without blocking the listener loop
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await HandleRequest(context);
                            }
                            catch (Exception Exception)
                            {
                                // Log any unhandled exceptions from request handling
                                await SSWEW.Watch_CatchException(Exception);
                            }
                        }, cancellationToken);
                    }
                }
                catch (HttpListenerException Exception) when (Exception.ErrorCode == 995) // ERROR_OPERATION_ABORTED
                {
                    // Listener was stopped, this is expected
                    break;
                }
                catch (HttpListenerException)
                {
                    // Other listener errors (e.g., "This operation is not supported")
                    // Break the loop to prevent continuous errors
                    break;
                }
                catch (ObjectDisposedException)
                {
                    // Listener was disposed
                    break;
                }
                catch (OperationCanceledException)
                {
                    // Cancellation requested
                    break;
                }
                catch (Exception Exception)
                {
                    // Unexpected error, log and continue
                    await SSWEW.Watch_CatchException(Exception);

                    // Check if we should continue
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(1000, cancellationToken); // Brief pause before retry
                    }
                }
            }
        }

        private async Task WriteFile(HttpListenerContext context, string path)
        {
            HttpListenerResponse response = context.Response;

            try
            {
                if (!File.Exists(path))
                {
                    return;
                }

                using FileStream fs = SSSHF.OpenRead(path);

                string filename = Path.GetFileName(path);

                response.ContentLength64 = fs.Length;

                response.SendChunked = true;
                response.ContentType = GetContentType(path);
                response.StatusCode = (int)HttpStatusCode.OK;
                response.AddHeader("Content-Disposition", $"inline; filename=\"{filename}\"");

                byte[] buffer = new byte[64 * 1024];
                int read;

                using Stream outputStream = response.OutputStream;

                while ((read = await fs.ReadAsync(buffer)) > 0)
                {
                    await outputStream.WriteAsync(buffer.AsMemory(0, read));
                    await outputStream.FlushAsync();
                }
            }
            catch (IOException)
            {
                // File read error or stream closed
            }
            catch (HttpListenerException)
            {
                // Response stream closed by client
            }
            catch (ObjectDisposedException)
            {
                // Response was disposed
            }
            catch (Exception)
            {
                // Unexpected error
            }
        }
    }
}
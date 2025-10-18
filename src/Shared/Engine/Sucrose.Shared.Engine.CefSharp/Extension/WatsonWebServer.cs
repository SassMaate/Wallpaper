using System.IO;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SSSHP = Sucrose.Shared.Space.Helper.Port;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Shared.Engine.CefSharp.Extension
{
    internal class WatsonWebServer(string themeFolder, string customFolder = null)
    {
        private readonly int Port = SSSHP.Available(SMMRG.Loopback);
        private CancellationTokenSource _cancellationTokenSource;
        private WebserverLite _server;
        private Task _serverTask;

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
                // Stop server
                if (_server != null)
                {
                    _server.Stop();
                    _server.Dispose();
                    _server = null;
                }
            }
            catch { }

            try
            {
                // Wait for server task to complete (with timeout)
                if (_serverTask != null && !_serverTask.IsCompleted)
                {
                    await _serverTask.WaitAsync(TimeSpan.FromSeconds(2));
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
                // Stop existing server if any
                await StopAsync();

                if (string.IsNullOrEmpty(customFolder))
                {
                    customFolder = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.Content);
                }

                _cancellationTokenSource = new CancellationTokenSource();

                // Configure Watson.Lite server settings
                WebserverSettings settings = new($"{SMMRG.Loopback}", Port);
                _server = new WebserverLite(settings, HandleRequest);

                // Start server in background task
                _serverTask = Task.Run(async () =>
                {
                    try
                    {
                        await _server.StartAsync(_cancellationTokenSource.Token);
                    }
                    catch (Exception Exception)
                    {
                        // Log any unhandled exceptions from the server
                        await SSWEW.Watch_CatchException(Exception);
                    }
                }, _cancellationTokenSource.Token);

                // Give the server a moment to start
                await Task.Delay(100);
            }
            catch (Exception Exception)
            {
                // Unexpected error during startup
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        private string GetContentType(string filename)
        {
            string extension = Path.GetExtension(filename).ToLower();

            return extension switch
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

        private async Task HandleRequest(HttpContextBase ctx)
        {
            if (ctx == null)
            {
                return;
            }

            try
            {
                // Set CORS headers
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                ctx.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS, PATCH");

                // Get the requested file path
                string relativePath = ctx.Request.Url.RawWithQuery.TrimStart('/');

                string themePath = Path.Combine(themeFolder, relativePath);
                string customPath = Path.Combine(customFolder, relativePath);

                string filePath = null;

                // Check if file exists in theme folder first, then custom folder
                if (File.Exists(themePath))
                {
                    filePath = themePath;
                }
                else if (File.Exists(customPath))
                {
                    filePath = customPath;
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    // Serve the file
                    await WriteFile(ctx, filePath);
                }
                else
                {
                    // File not found
                    ctx.Response.StatusCode = 404;
                    ctx.Response.ContentType = "text/plain; charset=utf-8";
                    await ctx.Response.Send("File not found.");
                }
            }
            catch (Exception Exception)
            {
                // Log error but don't crash
                await SSWEW.Watch_CatchException(Exception);

                try
                {
                    ctx.Response.StatusCode = 500;
                    await ctx.Response.Send("Internal server error.");
                }
                catch
                {
                    // Ignore if we can't send error response
                }
            }
        }

        private async Task WriteFile(HttpContextBase ctx, string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    ctx.Response.StatusCode = 404;
                    await ctx.Response.Send("File not found.");

                    return;
                }

                // Read file content
                string filename = Path.GetFileName(path);
                byte[] fileContent = await File.ReadAllBytesAsync(path);

                // Set response headers
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = GetContentType(path);
                ctx.Response.Headers.Add("Content-Disposition", $"inline; filename=\"{filename}\"");

                // Send file content
                await ctx.Response.Send(fileContent);
            }
            catch (IOException)
            {
                // File read error
                ctx.Response.StatusCode = 500;
                await ctx.Response.Send("Error reading file.");
            }
            catch (Exception)
            {
                // Unexpected error
                ctx.Response.StatusCode = 500;
                await ctx.Response.Send("Internal server error.");
            }
        }
    }
}
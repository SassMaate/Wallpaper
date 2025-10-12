using System.IO;
using System.Net;
using SEET = Skylark.Enum.EncodeType;
using SHE = Skylark.Helper.Encode;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SSSHP = Sucrose.Shared.Space.Helper.Port;

namespace Sucrose.Shared.Engine.Extension
{
    internal class LocalHttpServer(string themeFolder, string customFolder = null)
    {
        private readonly int Port = SSSHP.Available(SMMRG.Loopback);
        private readonly string Host = $"{SMMRG.Loopback}";
        private readonly HttpListener Listener = new();

        public string GetUrl()
        {
            return $"http://{Host}:{Port}/";
        }

        public async void StartAsync()
        {
            Listener.Prefixes.Add($"http://localhost:{Port}/");
            Listener.Prefixes.Add($"http://{SMMRG.Loopback}:{Port}/");

            if (string.IsNullOrEmpty(customFolder))
            {
                customFolder = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.Content);
            }

            if (Listener != null && !Listener.IsListening)
            {
                Listener.Start();

                await Task.Run(async () =>
                {
                    while (Listener.IsListening)
                    {
                        await Task.Run(async () =>
                        {
                            await HandleRequest(await Listener.GetContextAsync());
                        });
                    }
                });
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

        private async Task WriteFile(HttpListenerContext context, string path)
        {
            HttpListenerResponse response = context.Response;

            try
            {
                using FileStream fs = File.OpenRead(path);

                string filename = Path.GetFileName(path);

                response.ContentLength64 = fs.Length;

                response.SendChunked = true;
                response.ContentType = GetContentType(path);
                response.StatusCode = (int)HttpStatusCode.OK;
                response.AddHeader("Content-Disposition", $"inline; filename=\"{filename}\"");

                byte[] buffer = new byte[64 * 1024];
                int read;

                using Stream outputStream = response.OutputStream;

                while ((read = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await outputStream.WriteAsync(buffer.AsMemory(0, read));

                    await outputStream.FlushAsync();
                }
            }
            catch { }
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS, PATCH");

            string customPath = Path.Combine(customFolder, context.Request.Url.LocalPath.TrimStart('/'));
            string themePath = Path.Combine(themeFolder, context.Request.Url.LocalPath.TrimStart('/'));

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

            response.OutputStream.Close();

            response.Close();
        }

        public void Stop()
        {
            if (Listener != null && !Listener.IsListening)
            {
                Listener.Stop();
                Listener.Close();
            }
        }
    }
}
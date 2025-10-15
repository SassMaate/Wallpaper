using Newtonsoft.Json;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SSHD = Sucrose.Signal.Helper.Deleter;
using SSHR = Sucrose.Signal.Helper.Reader;
using SSHW = Sucrose.Signal.Helper.Writer;
using Timer = System.Timers.Timer;

namespace Sucrose.Signal
{
    public class SignalT(string Name)
    {
        private readonly JsonSerializerSettings SerializerSettings = new() { TypeNameHandling = TypeNameHandling.None, Formatting = Formatting.None };
        private readonly string Source = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.SignalT);
        private FileSystemWatcher FileWatcher;

        public FileSystemEventHandler CreatedEventHandler;

        public void StopChannel()
        {
            try
            {
                if (FileWatcher != null)
                {
                    FileWatcher.EnableRaisingEvents = false;
                    FileWatcher.Dispose();
                    FileWatcher = null;
                }
            }
            catch (Exception)
            {
                // Swallow exceptions during shutdown
            }
        }

        public void StartChannel(FileSystemEventHandler Handler)
        {
            try
            {
                CreatedEventHandler += Handler;

                // Ensure directory exists and clean up old files
                if (Directory.Exists(Source))
                {
                    try
                    {
                        string[] Files = Directory.GetFiles(Source, "*.*", SearchOption.TopDirectoryOnly);

                        foreach (string Record in Files)
                        {
                            if (FileCheck(Record))
                            {
                                try
                                {
                                    File.Delete(Record);
                                }
                                catch (IOException)
                                {
                                    // File is in use, skip
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    // No permission, skip
                                }
                            }
                        }
                    }
                    catch (DirectoryNotFoundException)
                    {
                        // Directory was deleted during operation, recreate
                        Directory.CreateDirectory(Source);
                    }
                }
                else
                {
                    Directory.CreateDirectory(Source);
                }

                // Stop existing watcher if any
                if (FileWatcher != null)
                {
                    StopChannel();
                }

                FileWatcher = new()
                {
                    Path = Source,
                    Filter = "*.*",
                    InternalBufferSize = 64 * 1024, // Increase buffer size to reduce overflow errors
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
                };

                FileWatcher.Error += (s, e) =>
                {
                    Exception Exception = e.GetException();

                    if (Exception is InternalBufferOverflowException)
                    {
                        // Buffer overflow, restart the watcher
                        try
                        {
                            FileWatcher.EnableRaisingEvents = true;
                            FileWatcher.EnableRaisingEvents = false;
                        }
                        catch { }
                    }
                };

                FileWatcher.Created += (s, e) =>
                {
                    try
                    {
                        if (FileCheck(e.FullPath))
                        {
                            CreatedEventHandler?.Invoke(s, e);
                        }
                    }
                    catch (Exception)
                    {
                        // Swallow exceptions in event handler to prevent watcher from stopping
                    }
                };

                FileWatcher.EnableRaisingEvents = true;
            }
            catch (Exception)
            {
                // Unexpected error during startup
            }
        }

        public void FileSave<T>(T Data)
        {
            try
            {
                // Ensure directory exists
                if (!Directory.Exists(Source))
                {
                    Directory.CreateDirectory(Source);
                }

                string Destination = Path.Combine(Source, $"{Path.GetFileNameWithoutExtension(Name)}-{Guid.NewGuid()}{Path.GetExtension(Name)}");

                SSHW.Write(Destination, JsonConvert.SerializeObject(Data, SerializerSettings));
            }
            catch (DirectoryNotFoundException)
            {
                // Directory was deleted, recreate and retry once
                Directory.CreateDirectory(Source);

                string Destination = Path.Combine(Source, $"{Path.GetFileNameWithoutExtension(Name)}-{Guid.NewGuid()}{Path.GetExtension(Name)}");

                SSHW.Write(Destination, JsonConvert.SerializeObject(Data, SerializerSettings));
            }
            catch (IOException)
            {
                // File is locked or in use
            }
            catch (UnauthorizedAccessException)
            {
                // No permission to write
            }
            catch (Exception)
            {
                // Unexpected error
            }
        }

        public string FileName(string Source)
        {
            return Path.GetFileName(Source);
        }

        public async void FileDelete(string Source)
        {
            await SSHD.Delete(Source);
        }

        public string FileRead(string Source, string Default, bool Delete = true)
        {
            try
            {
                string Data = SSHR.Read(Source);

                if (Delete)
                {
                    DeletionTimer(Source);
                }

                return Data;
            }
            catch (FileNotFoundException)
            {
                // File not found is expected in some cases
                return Default;
            }
            catch (IOException)
            {
                // File is locked or in use
                return Default;
            }
            catch (UnauthorizedAccessException)
            {
                // No permission to read
                return Default;
            }
            catch (Exception)
            {
                // Unexpected error
                return Default;
            }
        }

        public async Task<T> FileRead<T>(string Source, T Default, bool Delete = true)
        {
            try
            {
                // Small random delay to reduce contention
                await Task.Delay(SMMRG.Randomise.Next(5, 50));

                string Data = SSHR.Read(Source);

                if (string.IsNullOrWhiteSpace(Data))
                {
                    return Default;
                }

                if (Delete)
                {
                    DeletionTimer(Source);
                }

                return JsonConvert.DeserializeObject<T>(Data, SerializerSettings);
            }
            catch (FileNotFoundException)
            {
                // File not found is expected in some cases
                return Default;
            }
            catch (IOException)
            {
                // File is locked or in use
                return Default;
            }
            catch (UnauthorizedAccessException)
            {
                // No permission to read
                return Default;
            }
            catch (JsonException)
            {
                // Deserialization failed
                if (Delete && File.Exists(Source))
                {
                    DeletionTimer(Source); // Delete corrupt file
                }
                return Default;
            }
            catch (Exception)
            {
                // Unexpected error
                return Default;
            }
        }

        private bool FileCheck(string Source)
        {
            if (FileName(Source).Contains(Path.GetFileNameWithoutExtension(Name)) || FileName(Source) == Name)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void DeletionTimer(string Source)
        {
            Timer Deletion = new(3000);

            Deletion.Elapsed += (s, e) =>
            {
                FileDelete(Source);

                Deletion.Stop();
                Deletion.Dispose();
            };

            Deletion.AutoReset = false;

            Deletion.Start();
        }
    }
}
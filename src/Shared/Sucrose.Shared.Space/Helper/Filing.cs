using System.IO;
using SSSHU = Sucrose.Shared.Space.Helper.Unique;

namespace Sucrose.Shared.Space.Helper
{
    internal static class Filing
    {
        public static void Create(string Source)
        {
            using Mutex Mutex = new(false, SSSHU.GenerateText(Source));

            try
            {
                try
                {
                    Mutex.WaitOne();
                }
                catch { }

                File.Create(Source).Dispose();
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        public static void Delete(string Source)
        {
            using Mutex Mutex = new(false, SSSHU.GenerateText(Source));

            try
            {
                try
                {
                    Mutex.WaitOne();
                }
                catch { }

                File.Delete(Source);
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        public static string Read(string Source)
        {
            using Mutex Mutex = new(false, SSSHU.GenerateText(Source));

            try
            {
                try
                {
                    Mutex.WaitOne();
                }
                catch { }

                return File.ReadAllText(Source);
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        public static string ReadStream(string Source)
        {
            using Mutex Mutex = new(false, SSSHU.GenerateText(Source));

            try
            {
                try
                {
                    Mutex.WaitOne();
                }
                catch { }

                using FileStream Stream = new(Source, FileMode.Open, FileAccess.Read, FileShare.None);

                using StreamReader Reader = new(Stream);

                return Reader.ReadToEnd();
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        public static FileStream OpenRead(string Source)
        {
            using Mutex Mutex = new(false, SSSHU.GenerateText(Source));

            try
            {
                try
                {
                    Mutex.WaitOne();
                }
                catch { }

                return new(Source, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        public static void Write(string Source, string Content)
        {
            using Mutex Mutex = new(false, SSSHU.GenerateText(Source));

            try
            {
                try
                {
                    Mutex.WaitOne();
                }
                catch { }

                File.WriteAllText(Source, Content);
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        public static void Move(string Source, string Destination)
        {
            using Mutex SourceMutex = new(false, SSSHU.GenerateText(Source));
            using Mutex DestinationMutex = new(false, SSSHU.GenerateText(Destination));

            try
            {
                try
                {
                    SourceMutex.WaitOne();
                    DestinationMutex.WaitOne();
                }
                catch { }

                File.Move(Source, Destination);
            }
            finally
            {
                SourceMutex.ReleaseMutex();
                DestinationMutex.ReleaseMutex();
            }
        }

        public static void WriteStream(string Source, string Content)
        {
            using Mutex Mutex = new(false, SSSHU.GenerateText(Source));

            try
            {
                try
                {
                    Mutex.WaitOne();
                }
                catch { }

                FileMode FileMode = File.Exists(Source) ? FileMode.Truncate : FileMode.CreateNew;

                using FileStream Stream = new(Source, FileMode, FileAccess.Write, FileShare.None);

                using StreamWriter Writer = new(Stream);

                Writer.Write(Content);
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        public static void CopyStream(string Source, string Destination)
        {
            using Mutex SourceMutex = new(false, SSSHU.GenerateText(Source));
            using Mutex DestinationMutex = new(false, SSSHU.GenerateText(Destination));

            try
            {
                try
                {
                    SourceMutex.WaitOne();
                    DestinationMutex.WaitOne();
                }
                catch { }

                FileMode FileMode = File.Exists(Destination) ? FileMode.Truncate : FileMode.CreateNew;

                using FileStream SourceStream = new(Source, FileMode.Open, FileAccess.Read, FileShare.None);
                using FileStream DestinationStream = new(Destination, FileMode, FileAccess.Write, FileShare.None);

                SourceStream.CopyTo(DestinationStream);
            }
            finally
            {
                SourceMutex.ReleaseMutex();
                DestinationMutex.ReleaseMutex();
            }
        }

        public static string ReadBuffer(string Source, int BufferSize = 8192)
        {
            using Mutex Mutex = new(false, SSSHU.GenerateText(Source));

            try
            {
                try
                {
                    Mutex.WaitOne();
                }
                catch { }

                using FileStream Stream = new(Source, FileMode.Open, FileAccess.Read, FileShare.None);
                using StreamReader Reader = new(Stream);

                char[] Buffer = new char[BufferSize];

                int Length = Reader.Read(Buffer, 0, Buffer.Length);

                return new string(Buffer, 0, Length);
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        public static void Copy(string Source, string Destination, bool Overwrite = true)
        {
            using Mutex SourceMutex = new(false, SSSHU.GenerateText(Source));
            using Mutex DestinationMutex = new(false, SSSHU.GenerateText(Destination));

            try
            {
                try
                {
                    SourceMutex.WaitOne();
                    DestinationMutex.WaitOne();
                }
                catch { }

                File.Copy(Source, Destination, Overwrite);
            }
            finally
            {
                SourceMutex.ReleaseMutex();
                DestinationMutex.ReleaseMutex();
            }
        }

        public static void CopyInfo(string Source, string Destination, bool Overwrite = true)
        {
            using Mutex SourceMutex = new(false, SSSHU.GenerateText(Source));
            using Mutex DestinationMutex = new(false, SSSHU.GenerateText(Destination));

            try
            {
                try
                {
                    SourceMutex.WaitOne();
                    DestinationMutex.WaitOne();
                }
                catch { }

                FileInfo FileInfo = new(Source);

                FileInfo.CopyTo(Destination, Overwrite);
            }
            finally
            {
                SourceMutex.ReleaseMutex();
                DestinationMutex.ReleaseMutex();
            }
        }

        public static void CopyBuffer(string Source, string Destination, int BufferSize = 8192)
        {
            using Mutex SourceMutex = new(false, SSSHU.GenerateText(Source));
            using Mutex DestinationMutex = new(false, SSSHU.GenerateText(Destination));

            try
            {
                try
                {
                    SourceMutex.WaitOne();
                    DestinationMutex.WaitOne();
                }
                catch { }

                int BytesRead;

                byte[] Buffer = new byte[BufferSize];

                FileMode FileMode = File.Exists(Destination) ? FileMode.Truncate : FileMode.CreateNew;

                using FileStream SourceStream = new(Source, FileMode.Open, FileAccess.Read, FileShare.None);
                using FileStream DestinationStream = new(Destination, FileMode, FileAccess.Write, FileShare.None);

                while ((BytesRead = SourceStream.Read(Buffer, 0, Buffer.Length)) > 0)
                {
                    DestinationStream.Write(Buffer, 0, BytesRead);
                }
            }
            finally
            {
                SourceMutex.ReleaseMutex();
                DestinationMutex.ReleaseMutex();
            }
        }
    }
}
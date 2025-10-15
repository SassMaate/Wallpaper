namespace Sucrose.Signal.Helper
{
    internal static class Writer
    {
        private const int MAX_RETRIES = 3;
        private const int RETRY_DELAY_MS = 100;
        private const int MUTEX_TIMEOUT_MS = 5000; // 5 seconds timeout

        public static void Write(string filePath, string fileContent)
        {
            Exception lastException = null;

            // Ensure directory exists
            string directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
            {
                using Mutex mutex = new(false, $"SignalWriter_{Path.GetFileName(filePath)}");

                bool mutexAcquired = false;

                try
                {
                    // Try to acquire mutex with timeout
                    try
                    {
                        mutexAcquired = mutex.WaitOne(MUTEX_TIMEOUT_MS);

                        if (!mutexAcquired)
                        {
                            throw new TimeoutException($"Failed to acquire mutex for file: {filePath}");
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        // Another process abandoned the mutex, we can safely acquire it
                        mutexAcquired = true;
                    }

                    // Use Create instead of CreateNew to overwrite if exists
                    // But first check if we should use CreateNew for unique file names
                    FileMode mode = File.Exists(filePath) ? FileMode.Create : FileMode.CreateNew;

                    using FileStream fileStream = new(filePath, mode, FileAccess.Write, FileShare.None);
                    using StreamWriter writer = new(fileStream);

                    writer.Write(fileContent);
                    writer.Flush();

                    return; // Success, exit method
                }
                catch (IOException Exception) when (attempt < MAX_RETRIES - 1)
                {
                    // File might be locked by another process, retry
                    lastException = Exception;

                    Thread.Sleep(RETRY_DELAY_MS);
                }
                catch (UnauthorizedAccessException Exception) when (attempt < MAX_RETRIES - 1)
                {
                    // Access denied, retry
                    lastException = Exception;

                    Thread.Sleep(RETRY_DELAY_MS);
                }
                catch (Exception)
                {
                    // Non-retryable exception
                    throw;
                }
                finally
                {
                    if (mutexAcquired)
                    {
                        try
                        {
                            mutex.ReleaseMutex();
                        }
                        catch (ApplicationException)
                        {
                            // Mutex was not owned by this thread, ignore
                        }
                    }
                }
            }

            // All retries failed
            throw new InvalidOperationException($"Failed to write file after {MAX_RETRIES} attempts: {filePath}", lastException);
        }
    }
}
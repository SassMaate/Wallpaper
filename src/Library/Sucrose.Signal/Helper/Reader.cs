namespace Sucrose.Signal.Helper
{
    internal static class Reader
    {
        private const int MAX_RETRIES = 3;
        private const int RETRY_DELAY_MS = 100;
        private const int MUTEX_TIMEOUT_MS = 5000; // 5 seconds timeout

        public static string Read(string filePath)
        {
            Exception lastException = null;

            for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
            {
                using Mutex mutex = new(false, $"SignalReader_{Path.GetFileName(filePath)}");

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

                    // Check if file exists before trying to read
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException($"File not found: {filePath}");
                    }

                    using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using StreamReader reader = new(fileStream);

                    return reader.ReadToEnd();
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
            throw new InvalidOperationException($"Failed to read file after {MAX_RETRIES} attempts: {filePath}", lastException);
        }
    }
}
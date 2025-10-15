using SMMRG = Sucrose.Memory.Manage.Readonly.General;

namespace Sucrose.Signal.Helper
{
    internal static class Deleter
    {
        private const int MAX_RETRIES = 3;
        private const int MIN_RETRY_DELAY_MS = 50;
        private const int MAX_RETRY_DELAY_MS = 200;

        public static async Task Delete(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return; // Nothing to delete
            }

            Exception lastException = null;

            for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
            {
                try
                {
                    // Try to set file attributes to normal before deletion
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);

                    // Verify deletion
                    if (!File.Exists(filePath))
                    {
                        return; // Success
                    }

                    // File still exists, wait and retry
                    await Task.Delay(SMMRG.Randomise.Next(MIN_RETRY_DELAY_MS, MAX_RETRY_DELAY_MS));
                }
                catch (IOException Exception)
                {
                    // File is in use or locked
                    lastException = Exception;

                    if (attempt < MAX_RETRIES - 1)
                    {
                        await Task.Delay(SMMRG.Randomise.Next(MIN_RETRY_DELAY_MS, MAX_RETRY_DELAY_MS));
                    }
                }
                catch (UnauthorizedAccessException Exception)
                {
                    // No permission or file is read-only
                    lastException = Exception;

                    if (attempt < MAX_RETRIES - 1)
                    {
                        try
                        {
                            // Try to remove read-only attribute
                            File.SetAttributes(filePath, FileAttributes.Normal);
                        }
                        catch { }

                        await Task.Delay(SMMRG.Randomise.Next(MIN_RETRY_DELAY_MS, MAX_RETRY_DELAY_MS));
                    }
                }
                catch (Exception Exception)
                {
                    // Unexpected error, log it but don't throw
                    lastException = Exception;

                    break;
                }
            }

            // All retries failed, but we don't throw - just swallow the error
            // This is to maintain backward compatibility with the original behavior
        }
    }
}
namespace Sucrose.Shared.Store.Helper
{
    internal sealed class Counter
    {
        public long DownloadedSize { get; set; } = 0;

        public double ReportedPercentage { get; set; } = -1;
    }
}
using SPSSS = Sucrose.Portal.Services.StoreService;

namespace Sucrose.Shared.Store.Manage
{
    internal static class Internal
    {
        public static bool State = true;

        public static SPSSS StoreService { get; } = new();
    }
}
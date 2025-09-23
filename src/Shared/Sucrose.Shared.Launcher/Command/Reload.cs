using SELLT = Skylark.Enum.LevelLogType;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SSDECT = Sucrose.Shared.Dependency.Enum.CommandType;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SSSMI = Sucrose.Shared.Space.Manage.Internal;

namespace Sucrose.Shared.Launcher.Command
{
    internal static class Reload
    {
        public static void Command()
        {
            SMMI.LauncherLogManager.Log(SELLT.Info, "Wallpaper is being reloaded...");

            SSSHP.Run(SSSMI.Commandog, $"{SMMRG.StartCommand}{SSDECT.RestartLive}{SMMRG.ValueSeparator}{SMMRG.Unknown}");

            SMMI.LauncherLogManager.Log(SELLT.Info, "Wallpaper has been reloaded.");
        }
    }
}
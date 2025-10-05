using SSSMI = Sucrose.Shared.Space.Manage.Internal;

namespace Sucrose.Shared.Dependency.Helper
{
    internal static class Runtime
    {
        public static void Configure()
        {
#if RELEASE
            Environment.SetEnvironmentVariable("DOTNET_ROOT", SSSMI.Runtime, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("DOTNET_MULTILEVEL_LOOKUP", "0", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("DOTNET_ROLL_FORWARD", "LatestMajor", EnvironmentVariableTarget.Process);

            Environment.SetEnvironmentVariable("PATH", $"{SSSMI.Runtime};{SSSMI.This}", EnvironmentVariableTarget.Process);
#endif
        }
    }
}
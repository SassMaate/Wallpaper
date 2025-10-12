namespace Sucrose.Shared.Space.Helper
{
    internal static class Virtual
    {
        public static List<string> GetApp()
        {
            return new()
            {
                "nox.exe", "noxplayer.exe", "noxvmhandle.exe",
                "memu.exe", "MEmuPlayer.exe", "ldplayer.exe", "ldplayer7.exe", "ldconsole.exe",
                "bluestacks.exe", "hd-player.exe", "HD-Player.exe", "HD-Agent.exe",
                "dnplayer.exe", "droid4x.exe", "andyd.exe", "andy.exe", "genymotion.exe", "player.exe",
                "tencent_emulator.exe",

                "vmware.exe", "vmplayer.exe", "vmware-vmx.exe", "vmware-authd.exe",
                "virtualbox.exe", "virtualboxvm.exe", "VBoxSVC.exe", "VBoxService.exe", "VBoxHeadless.exe", "vboxmanage.exe",
                "qemu-system-x86_64.exe", "qemu-system.exe", "qemu.exe",

                "vmms.exe", "vmconnect.exe", "vmwp.exe",

                "windowssandbox.exe", "WindowsSandbox.exe", "WindowsSandboxClient.exe",
                "sandboxie.exe", "SbieSvc.exe"
            };
        }
    }
}
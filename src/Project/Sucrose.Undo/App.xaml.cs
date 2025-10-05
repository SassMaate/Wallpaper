using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using SHC = Skylark.Helper.Culture;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SRER = Sucrose.Resources.Extension.Resources;
using SRHR = Sucrose.Resources.Helper.Resources;
using SWUD = Skylark.Wing.Utility.Desktop;

namespace Sucrose.Undo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string Message => SRER.GetValue("Undo", "QuestionMessage") + Environment.NewLine + Environment.NewLine + SRER.GetValue("Undo", "QuestionDescription");

        private static string Runtime => Path.Combine(UninstallPath, $"{SMMRG.AppName}.{SMMRF.Runtime}");

        private static string UninstallPath => Path.Combine(SMMRP.LocalApplicationData, SMMRG.AppName);

        private static string UninstallDataPath => Path.Combine(SMMRP.ApplicationData, SMMRG.AppName);

        private static string RegistryName => @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

        private static string BatchFile = Path.Combine(SMMRP.Temp, $"del_{Guid.NewGuid():N}.bat");

        private static string StartMenu => Path.Combine(SMMRP.StartMenu, "Programs", Shortcut);

        private static string Undo => Path.Combine(UninstallPath, $"{SMMRG.AppName}.Undo");

        private static string Desktop => Path.Combine(SMMRP.Desktop, Shortcut);

        private static string Title => SRER.GetValue("Undo", "QuestionTitle");

        private static string Shortcut => $"{SMMRG.AppLongName}.lnk";

        private static int Delay => 1000;

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private static void DeleteDirectory(string Location)
        {
            if (Directory.Exists(Location))
            {
                string[] Files = Directory.GetFiles(Location, "*", SearchOption.AllDirectories);

                if (Files.Any())
                {
                    foreach (string Record in Files)
                    {
                        if (!Record.StartsWith(Undo, StringComparison.OrdinalIgnoreCase) && !Record.StartsWith(Runtime, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                File.Delete(Record);
                            }
                            catch { }
                        }
                    }
                }

                string[] Folders = Directory.GetDirectories(Location, "*", SearchOption.AllDirectories).OrderByDescending(Folder => Folder.Length).ToArray();

                if (Folders.Any())
                {
                    foreach (string Record in Folders)
                    {
                        if (!Record.StartsWith(Undo, StringComparison.OrdinalIgnoreCase) && !Record.StartsWith(Runtime, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                Directory.Delete(Record);
                            }
                            catch { }
                        }
                    }
                }

                try
                {
                    Directory.Delete(Location);
                }
                catch { }
            }
        }

        private static void TerminateProcess(string Name)
        {
            IEnumerable<Process> Processes = Process.GetProcesses().Where(Proc => Proc.ProcessName.Contains(Name) && Proc.Id != Environment.ProcessId);

            foreach (Process Process in Processes)
            {
                try
                {
                    Process.Kill();
                }
                catch { }
            }
        }

        private static void DeleteSelf()
        {
            try
            {
                StringBuilder BatchContent = new();

                BatchContent.AppendLine("@echo off");
                BatchContent.AppendLine("setlocal enabledelayedexpansion");
                BatchContent.AppendLine($"taskkill /PID {Environment.ProcessId} /T /F > nul 2>&1");
                BatchContent.AppendLine("timeout /t 3 /nobreak > nul");

                BatchContent.AppendLine($@"rd /s /q ""{Undo}"" > nul 2>&1");
                BatchContent.AppendLine($@"rd /s /q ""{Runtime}"" > nul 2>&1");
                BatchContent.AppendLine($@"rd /s /q ""{UninstallPath}"" > nul 2>&1");

                BatchContent.AppendLine(@"del ""%~f0"" > nul 2>&1");
                BatchContent.AppendLine("endlocal");
                BatchContent.AppendLine("exit");

                File.WriteAllText(BatchFile, BatchContent.ToString(), Encoding.ASCII);

                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WorkingDirectory = SMMRP.Temp,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = $"/c start /B \"\" \"{BatchFile}\""
                });
            }
            catch { }
        }

        protected void Close()
        {
            //Process.GetCurrentProcess().Close();
            //Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
            Current.Shutdown();
            Shutdown();
        }

        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MessageBoxResult Result = MessageBoxResult.Cancel;

            SRHR.SetLanguage(SHC.CurrentUITwoLetterISOLanguageName);

            if (!e.Args.Any())
            {
                SystemSounds.Asterisk.Play();

                Result = MessageBox.Show(Message, Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            }

            if (Result is MessageBoxResult.Yes or MessageBoxResult.No)
            {
                await Task.Delay(Delay);

                TerminateProcess(SMMRG.AppName);

                await Task.Delay(Delay);

                TerminateProcess(SMMRG.AppName);

                await Task.Delay(Delay);

                SWUD.RefreshDesktop();

                await Task.Delay(Delay);

                DeleteDirectory(UninstallPath);

                if (Result == MessageBoxResult.Yes)
                {
                    await Task.Delay(Delay);

                    DeleteDirectory(UninstallDataPath);
                }

                await Task.Delay(Delay);

                if (File.Exists(Desktop))
                {
                    File.Delete(Desktop);
                }

                if (File.Exists(StartMenu))
                {
                    File.Delete(StartMenu);
                }

                await Task.Delay(Delay);

                RegistryKey HomeKey = Registry.CurrentUser.OpenSubKey(RegistryName, true);
                HomeKey?.DeleteSubKey(SMMRG.AppName, false);

                await Task.Delay(Delay);

                DeleteSelf();
            }

            Close();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception Exception = (Exception)e.ExceptionObject;

            if (Exception != null)
            {
                SystemSounds.Exclamation.Play();

                MessageBox.Show(Exception.Message + Environment.NewLine + Environment.NewLine + Exception.StackTrace, "Error Information", MessageBoxButton.OK, MessageBoxImage.Error);

                Close();
            }
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception Exception = e.Exception;

            if (Exception != null)
            {
                SystemSounds.Exclamation.Play();

                MessageBox.Show(Exception.Message + Environment.NewLine + Environment.NewLine + Exception.StackTrace, "Error Information", MessageBoxButton.OK, MessageBoxImage.Error);

                e.Handled = true;

                Close();
            }
        }
    }
}
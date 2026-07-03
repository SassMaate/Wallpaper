using Newtonsoft.Json;
using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using SELLT = Skylark.Enum.LevelLogType;
using SEWTT = Skylark.Enum.WindowsThemeType;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMML = Sucrose.Manager.Manage.Library;
using SPCB = Sucrose.Property.Controls.Button;
using SPCCB = Sucrose.Property.Controls.CheckBox;
using SPCCP = Sucrose.Property.Controls.ColorPicker;
using SPCDD = Sucrose.Property.Controls.DropDown;
using SPCFDD = Sucrose.Property.Controls.FileDropDown;
using SPCL = Sucrose.Property.Controls.Label;
using SPCNB = Sucrose.Property.Controls.NumberBox;
using SPCPB = Sucrose.Property.Controls.PasswordBox;
using SPCS = Sucrose.Property.Controls.Slider;
using SPCTB = Sucrose.Property.Controls.TextBox;
using SPMI = Sucrose.Property.Manage.Internal;
using SPMMP = Sucrose.Property.Manage.Manager.Portal;
using SRER = Sucrose.Resources.Extension.Resources;
using SSDMMG = Sucrose.Shared.Dependency.Manage.Manager.General;
using SSLHK = Sucrose.Shared.Live.Helper.Kill;
using SSLHR = Sucrose.Shared.Live.Helper.Run;
using SSSHF = Sucrose.Shared.Space.Helper.Filing;
using SSSHL = Sucrose.Shared.Space.Helper.Live;
using SSTHP = Sucrose.Shared.Theme.Helper.Properties;
using SSTMBM = Sucrose.Shared.Theme.Model.ButtonModel;
using SSTMCBM = Sucrose.Shared.Theme.Model.CheckBoxModel;
using SSTMCM = Sucrose.Shared.Theme.Model.ControlModel;
using SSTMCPM = Sucrose.Shared.Theme.Model.ColorPickerModel;
using SSTMDDM = Sucrose.Shared.Theme.Model.DropDownModel;
using SSTMFDDM = Sucrose.Shared.Theme.Model.FileDropDownModel;
using SSTMLM = Sucrose.Shared.Theme.Model.LabelModel;
using SSTMNBM = Sucrose.Shared.Theme.Model.NumberBoxModel;
using SSTMPBM = Sucrose.Shared.Theme.Model.PasswordBoxModel;
using SSTMSM = Sucrose.Shared.Theme.Model.SliderModel;
using SSTMTBM = Sucrose.Shared.Theme.Model.TextBoxModel;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;
using SWHWT = Skylark.Wing.Helper.WindowsTheme;
using SWHWTR = Skylark.Wing.Helper.WindowsTaskbar;

namespace Sucrose.Property.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            if (SPMMP.BackdropType == WindowBackdropType.Auto)
            {
                if (SWHWT.GetTheme() == SEWTT.Dark)
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                }
                else
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                }
            }
            else
            {
                if (SSDMMG.ThemeType == SEWTT.Dark)
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                }
                else
                {
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                }
            }
        }

        private void Container_Controls()
        {
            Container.Children.Clear();

            foreach (KeyValuePair<string, SSTMCM> Pair in SPMI.Properties.PropertyList)
            {
                Container.Children.Add(Pair.Value.Type.ToLower() switch
                {
                    "label" => new SPCL(Pair.Value as SSTMLM),
                    "button" => new SPCB(Pair.Key, Pair.Value as SSTMBM),
                    "slider" => new SPCS(Pair.Key, Pair.Value as SSTMSM),
                    "textbox" => new SPCTB(Pair.Key, Pair.Value as SSTMTBM),
                    "checkbox" => new SPCCB(Pair.Key, Pair.Value as SSTMCBM),
                    "dropdown" => new SPCDD(Pair.Key, Pair.Value as SSTMDDM),
                    "numberbox" => new SPCNB(Pair.Key, Pair.Value as SSTMNBM),
                    "passwordbox" => new SPCPB(Pair.Key, Pair.Value as SSTMPBM),
                    "filedropdown" => new SPCFDD(Pair.Key, Pair.Value as SSTMFDDM),
                    "colorpicker" => new SPCCP(Pair.Key, Pair.Value as SSTMCPM, Restore),
                    _ => throw new NotSupportedException(string.Format(SRER.GetValue("Property", "Type", "NotSupport"), Pair.Value.Type)),
                });
            }
        }

        private async void MainWindow_Calculate()
        {
            double DpiScaleX = 1.0;
            double DpiScaleY = 1.0;

            double ScreenWidth = SystemParameters.PrimaryScreenWidth;
            double ScreenHeight = SystemParameters.PrimaryScreenHeight;

            PresentationSource Source = PresentationSource.FromVisual(this);

            if (Source?.CompositionTarget != null)
            {
                DpiScaleX = Source.CompositionTarget.TransformToDevice.M11;
                DpiScaleY = Source.CompositionTarget.TransformToDevice.M22;
            }

            AnchorStyles Anchor = SWHWTR.GetAnchorStyle(false);
            Rectangle TaskbarCoordinates = SWHWTR.GetCoordonates();

            double TaskbarTop = TaskbarCoordinates.Top / DpiScaleY;
            double TaskbarLeft = TaskbarCoordinates.Left / DpiScaleX;
            double TaskbarWidth = TaskbarCoordinates.Width / DpiScaleX;
            double TaskbarHeight = TaskbarCoordinates.Height / DpiScaleY;

            bool IsRTL = FlowDirection == System.Windows.FlowDirection.RightToLeft;

            try
            {
                switch (Anchor)
                {
                    case AnchorStyles.Top:
                        MaxHeight = ScreenHeight - TaskbarHeight - 20;

                        Left = IsRTL ? 10 : ScreenWidth - Width - 10;
                        Top = ScreenHeight - Height - 10;
                        break;
                    case AnchorStyles.Bottom:
                        MaxHeight = ScreenHeight - TaskbarHeight - 20;

                        Left = IsRTL ? 10 : ScreenWidth - Width - 10;
                        Top = TaskbarTop - Height - 10;
                        break;
                    case AnchorStyles.Left:
                        MaxHeight = ScreenHeight - 20;

                        Left = IsRTL ? TaskbarWidth + 10 : ScreenWidth - Width - 10;
                        Top = ScreenHeight - Height - 10;
                        break;
                    case AnchorStyles.Right:
                        MaxHeight = ScreenHeight - 20;

                        Left = IsRTL ? 10 : TaskbarLeft - Width - 10;
                        Top = ScreenHeight - Height - 10;
                        break;
                    default:
                        MaxHeight = ScreenHeight - 20;

                        Left = IsRTL ? 10 : ScreenWidth - Width - 10;
                        Top = ScreenHeight - Height - 10;
                        break;
                }
            }
            catch (Exception Exception)
            {
                SMMI.PropertyLogManager.Log(SELLT.Debug, $"Calculate: {JsonConvert.SerializeObject(new Hashtable()
                {
                    { "Anchor", $"{Anchor}" },
                    { "DPI Scale X", DpiScaleX },
                    { "DPI Scale Y", DpiScaleY },
                    { "Screen Width", ScreenWidth },
                    { "Screen Height", ScreenHeight },
                    { "Flow Direction", FlowDirection },
                    { "Taskbar Coordinates", SWHWTR.GetCoordonates() }
                })}");

                await SSWEW.Watch_CatchException(Exception);

                MaxHeight = ScreenHeight - 48 - 20;

                Top = ScreenHeight - Height - 58;
                Left = IsRTL ? 10 : ScreenWidth - Width - 10;
            }
        }

        private static WindowBackdropType GetWindowBackdropType()
        {
            if (WindowBackdrop.IsSupported(SPMMP.BackdropType))
            {
                return SPMMP.BackdropType;
            }
            else
            {
                return SPMI.DefaultBackdropType;
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(SPMI.Path))
            {
                Restore.IsEnabled = false;
                Refresh.IsEnabled = false;
                Delete.IsEnabled = false;

                SPMI.EngineLive = SMML.Selected == SPMI.LibrarySelected && SSSHL.Run();

                if (SPMI.EngineLive)
                {
                    SSLHK.Stop();
                }

                await Task.Delay(250);

                SSSHF.Delete(SPMI.PropertiesFile);

                await Task.Delay(250);

                SSSHF.CopyBuffer(SPMI.PropertiesPath, SPMI.PropertiesFile);

                SPMI.Properties = SSTHP.ReadJson(SPMI.PropertiesFile);

                if (SPMI.EngineLive)
                {
                    await Task.Delay(250);

                    SSLHR.Start();
                }

                await Task.Delay(250);

                Container_Controls();

                await Task.Delay(250);

                MainWindow_Calculate();

                await Task.Delay(250);

                SPMI.EngineLive = false;

                Delete.IsEnabled = true;
                Refresh.IsEnabled = true;
                Restore.IsEnabled = true;
            }
            else
            {
                Close();
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(SPMI.Path))
            {
                Restore.IsEnabled = false;
                Refresh.IsEnabled = false;
                Delete.IsEnabled = false;

                SPMI.Properties = SSTHP.ReadJson(SPMI.PropertiesFile);

                await Task.Delay(250);

                Container_Controls();

                await Task.Delay(250);

                MainWindow_Calculate();

                await Task.Delay(250);

                Delete.IsEnabled = true;
                Refresh.IsEnabled = true;
                Restore.IsEnabled = true;
            }
            else
            {
                Close();
            }
        }

        private async void Restore_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(SPMI.Path))
            {
                Restore.IsEnabled = false;
                Refresh.IsEnabled = false;
                Delete.IsEnabled = false;

                SSSHF.CopyBuffer(SPMI.PropertiesPath, SPMI.PropertiesFile);

                if (SMML.Selected == SPMI.LibrarySelected && SSSHL.Run())
                {
                    await Task.Delay(250);

                    SSSHF.CopyBuffer(SPMI.PropertiesPath, SPMI.WatcherFile.Replace("*", $"{Guid.NewGuid()}"));
                }

                await Task.Delay(250);

                SPMI.Properties = SSTHP.ReadJson(SPMI.PropertiesFile);

                await Task.Delay(250);

                Container_Controls();

                await Task.Delay(250);

                MainWindow_Calculate();

                await Task.Delay(250);

                Delete.IsEnabled = true;
                Refresh.IsEnabled = true;
                Restore.IsEnabled = true;
            }
            else
            {
                Close();
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Container_Controls();

            WindowBackdropType = GetWindowBackdropType();

            await Task.Delay(500);

            MainWindow_Calculate();

            ShowInTaskbar = true;
        }
    }
}
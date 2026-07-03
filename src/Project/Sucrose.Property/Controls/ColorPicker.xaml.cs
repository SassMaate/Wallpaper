using HandyControl.Controls;
using HandyControl.Themes;
using HandyControl.Tools;
using System.Windows;
using Wpf.Ui.Controls;
using Control = System.Windows.Controls.Control;
using CPicker = HandyControl.Controls.ColorPicker;
using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;
using SEWTT = Skylark.Enum.WindowsThemeType;
using SPHL = Sucrose.Property.Helper.Localization;
using SPHP = Sucrose.Property.Helper.Properties;
using SPMMP = Sucrose.Property.Manage.Manager.Portal;
using SRER = Sucrose.Resources.Extension.Resources;
using SSDMMG = Sucrose.Shared.Dependency.Manage.Manager.General;
using SSECCE = Skylark.Standard.Extension.Color.ColorExtension;
using SSTMCPM = Sucrose.Shared.Theme.Model.ColorPickerModel;
using SWHWT = Skylark.Wing.Helper.WindowsTheme;
using ToolTip = System.Windows.Controls.ToolTip;
using UserControl = System.Windows.Controls.UserControl;

namespace Sucrose.Property.Controls
{
    /// <summary>
    /// ColorPicker.xaml etkileşim mantığı
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        private Control Control { get; set; }

        public ColorPicker(string Key, SSTMCPM Data, Control Control)
        {
            InitializeComponent();

            this.Control = Control;

            InitializeData(Key, Data);

            if (SPMMP.BackdropType == WindowBackdropType.Auto)
            {
                if (SWHWT.GetTheme() == SEWTT.Dark)
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                }
                else
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                }
            }
            else
            {
                if (SSDMMG.ThemeType == SEWTT.Dark)
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                }
                else
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                }
            }
        }

        private void Command_Click(string Key, SSTMCPM Data)
        {
            CPicker Picker = SingleOpenHelper.CreateControl<CPicker>();

            MediaColor UndoColor = Component.Color;

            Picker.SelectedBrush = new(UndoColor);
            Picker.FlowDirection = FlowDirection;
            Picker.UseLayoutRounding = true;

            PopupWindow PopupWindow = new()
            {
                Title = SRER.GetValue("Property", "ColorPicker", "Popup", "Title"),
                WindowStartupLocation = WindowStartupLocation.Manual,
                WindowState = WindowState.Normal,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                FlowDirection = FlowDirection,
                AllowsTransparency = true,
                UseLayoutRounding = true,
                PopupElement = Picker,
                ShowActivated = true,
                ShowCancel = true,
                ShowBorder = true,
                Focusable = true,
                Topmost = true
            };

            Picker.SelectedColorChanged += (s, e) =>
            {
                Component.Color = e.Info;
            };

            Picker.Confirmed += (s, e) =>
            {
                Data.Value = e.Info.ToString();

                Component.Color = e.Info;

                SPHP.Change(Key, Data);

                PopupWindow.Close();
            };

            Picker.Canceled += delegate
            {
                Component.Color = UndoColor;

                PopupWindow.Close();
            };

            PopupWindow.Show(Control, false);
        }

        private void InitializeData(string Key, SSTMCPM Data)
        {
            Label.Text = SPHL.Convert(Data.Text);
            DrawingColor Color = SSECCE.HexToColor(SPHL.Convert(Data.Value));
            Component.Color = MediaColor.FromArgb(Color.A, Color.R, Color.G, Color.B);

            Command.Click += (s, e) => Command_Click(Key, Data);

            if (!string.IsNullOrEmpty(Data.Help))
            {
                ToolTip HelpTip = new()
                {
                    Content = SPHL.Convert(Data.Help)
                };

                Container.ToolTip = HelpTip;
            }
        }
    }
}
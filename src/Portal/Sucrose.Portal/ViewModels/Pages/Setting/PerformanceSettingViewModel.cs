using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;
using SEST = Skylark.Enum.StorageType;
using SMMB = Sucrose.Manager.Manage.Backgroundog;
using SMMCB = Sucrose.Memory.Manage.Constant.Backgroundog;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMRA = Sucrose.Memory.Manage.Readonly.App;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMS = Sucrose.Manager.Manage.System;
using SPVCEC = Sucrose.Portal.Views.Controls.ExpanderCard;
using SRER = Sucrose.Resources.Extension.Resources;
using SSDECDT = Sucrose.Shared.Dependency.Enum.CommandType;
using SSDECNT = Sucrose.Shared.Dependency.Enum.CommunicationType;
using SSDEPPT = Sucrose.Shared.Dependency.Enum.PausePerformanceType;
using SSDEPT = Sucrose.Shared.Dependency.Enum.PerformanceType;
using SSDMMB = Sucrose.Shared.Dependency.Manage.Manager.Backgroundog;
using SSDSH = Sucrose.Shared.Dependency.Struct.Host;
using SSSHN = Sucrose.Shared.Space.Helper.Network;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SSSMI = Sucrose.Shared.Space.Manage.Internal;
using TextBlock = System.Windows.Controls.TextBlock;

namespace Sucrose.Portal.ViewModels.Pages
{
    public partial class PerformanceSettingViewModel : ViewModel, IDisposable
    {
        [ObservableProperty]
        private List<UIElement> _Contents = [];

        private bool _isInitialized;

        public PerformanceSettingViewModel()
        {
            if (!_isInitialized)
            {
                InitializeViewModel();
            }
        }

        private void InitializeViewModel()
        {
            TextBlock AppearanceBehaviorArea = new()
            {
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                Text = SRER.GetValue("Portal", "Area", "AppearanceBehavior"),
                Margin = new Thickness(0, 0, 0, 0),
                FontWeight = FontWeights.Bold
            };

            Contents.Add(AppearanceBehaviorArea);

            SPVCEC Counter = new()
            {
                Expandable = !SMMB.PerformanceCounter,
                IsExpand = !SMMB.PerformanceCounter,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Counter.LeftIcon.Symbol = SymbolRegular.ShiftsActivity24;
            Counter.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Counter");
            Counter.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Counter", "Description");

            ToggleSwitch CounterState = new()
            {
                IsChecked = SMMB.PerformanceCounter
            };

            CounterState.Checked += (s, e) => CounterStateChecked(Counter, true);
            CounterState.Unchecked += (s, e) => CounterStateChecked(Counter, false);

            Counter.HeaderFrame = CounterState;

            TextBlock CounterHint = new()
            {
                Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Counter", "CounterHint"),
                Foreground = SRER.GetResource<Brush>("TextFillColorSecondaryBrush"),
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.WrapWithOverflow,
                TextAlignment = TextAlignment.Left,
                Margin = new Thickness(0, 0, 0, 0),
                FontWeight = FontWeights.SemiBold
            };

            Counter.FooterCard = CounterHint;

            Contents.Add(Counter);

            SPVCEC Pause = new()
            {
                Expandable = false,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Pause.LeftIcon.Symbol = SymbolRegular.PauseSettings20;
            Pause.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Pause");
            Pause.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Pause", "Description");

            ComboBox PausePerformanceType = new();

            PausePerformanceType.SelectionChanged += (s, e) => PausePerformanceTypeSelected(PausePerformanceType.SelectedIndex);

            foreach (SSDEPPT Type in Enum.GetValues(typeof(SSDEPPT)))
            {
                PausePerformanceType.Items.Add(SRER.GetValue("Portal", "Enum", "PausePerformanceType", $"{Type}"));
            }

            PausePerformanceType.SelectedIndex = (int)SSDMMB.PausePerformanceType;

            Pause.HeaderFrame = PausePerformanceType;

            Contents.Add(Pause);

            SPVCEC Communication = new()
            {
                Expandable = false,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Communication.LeftIcon.Symbol = SymbolRegular.Communication24;
            Communication.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Communication");
            Communication.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Communication", "Description");

            ComboBox CommunicationType = new();

            CommunicationType.SelectionChanged += (s, e) => CommunicationTypeSelected(CommunicationType.SelectedIndex);

            foreach (SSDECNT Type in Enum.GetValues(typeof(SSDECNT)))
            {
                CommunicationType.Items.Add(SRER.GetValue("Portal", "Enum", "CommunicationType", $"{Type}"));
            }

            CommunicationType.SelectedIndex = (int)SSDMMB.CommunicationType;

            Communication.HeaderFrame = CommunicationType;

            Contents.Add(Communication);

            TextBlock SystemResourcesArea = new()
            {
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                Text = SRER.GetValue("Portal", "Area", "SystemResources"),
                Margin = new Thickness(0, 10, 0, 0),
                FontWeight = FontWeights.Bold
            };

            Contents.Add(SystemResourcesArea);

            SPVCEC Processor = new()
            {
                Margin = new Thickness(0, 10, 0, 0)
            };

            Processor.LeftIcon.Symbol = SymbolRegular.DeveloperBoard24;
            Processor.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Processor");
            Processor.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Processor", "Description");

            ComboBox ProcessorPerformance = new();

            ProcessorPerformance.SelectionChanged += (s, e) => ProcessorPerformanceSelected(ProcessorPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                ProcessorPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            ProcessorPerformance.SelectedIndex = (int)SSDMMB.ProcessorPerformance;

            Processor.HeaderFrame = ProcessorPerformance;

            StackPanel ProcessorContent = new()
            {
                Orientation = Orientation.Horizontal
            };

            TextBlock ProcessorUsageText = new()
            {
                Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Processor", "ProcessorUsage"),
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.SemiBold
            };

            NumberBox ProcessorUsage = new()
            {
                Icon = new SymbolIcon(SymbolRegular.ArrowTrendingLines24),
                IconPlacement = ElementPlacement.Left,
                Margin = new Thickness(0, 0, 10, 0),
                Value = SMMB.ProcessorUsage,
                ClearButtonEnabled = false,
                MaxDecimalPlaces = 0,
                Maximum = 100,
                MaxLength = 3,
                Minimum = 0
            };

            ProcessorUsage.ValueChanged += (s, e) => ProcessorUsageChanged(ProcessorUsage.Value);

            ProcessorContent.Children.Add(ProcessorUsageText);
            ProcessorContent.Children.Add(ProcessorUsage);

            Processor.FooterCard = ProcessorContent;

            Contents.Add(Processor);

            SPVCEC Graphic = new()
            {
                Margin = new Thickness(0, 10, 0, 0)
            };

            Graphic.LeftIcon.Symbol = SymbolRegular.VideoPerson24;
            Graphic.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Graphic");
            Graphic.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Graphic", "Description");

            ComboBox GraphicPerformance = new();

            GraphicPerformance.SelectionChanged += (s, e) => GraphicPerformanceSelected(GraphicPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                GraphicPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            GraphicPerformance.SelectedIndex = (int)SSDMMB.GraphicPerformance;

            Graphic.HeaderFrame = GraphicPerformance;

            StackPanel GraphicContent = new();

            StackPanel GraphicAdapterContent = new()
            {
                Orientation = Orientation.Horizontal
            };

            TextBlock GraphicAdapterText = new()
            {
                Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Graphic", "GraphicAdapter"),
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.SemiBold
            };

            ComboBox GraphicAdapter = new()
            {
                MaxDropDownHeight = 200,
                MaxWidth = 700
            };

            DynamicScrollViewer.SetVerticalScrollBarVisibility(GraphicAdapter, ScrollBarVisibility.Auto);

            if (SMMS.GraphicInterfaces.Any())
            {
                GraphicAdapter.SelectionChanged += (s, e) => GraphicAdapterSelected($"{GraphicAdapter.SelectedValue}");

                foreach (string Interface in SMMS.GraphicInterfaces)
                {
                    GraphicAdapter.Items.Add(Interface);
                }

                string SelectedAdapter = SMMB.GraphicAdapter;

                if (string.IsNullOrEmpty(SelectedAdapter))
                {
                    GraphicAdapter.SelectedIndex = 0;
                }
                else
                {
                    GraphicAdapter.SelectedValue = SelectedAdapter;
                }
            }
            else
            {
                GraphicAdapter.Items.Add(new ComboBoxItem()
                {
                    Content = SRER.GetValue("Portal", "PerformanceSettingPage", "Graphic", "GraphicAdapter", "Empty"),
                    IsSelected = true
                });
            }

            StackPanel GraphicUsageContent = new()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            TextBlock GraphicUsageText = new()
            {
                Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Graphic", "GraphicUsage"),
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.SemiBold
            };

            NumberBox GraphicUsage = new()
            {
                Icon = new SymbolIcon(SymbolRegular.ArrowTrendingSparkle24),
                IconPlacement = ElementPlacement.Left,
                Margin = new Thickness(0, 0, 10, 0),
                ClearButtonEnabled = false,
                Value = SMMB.GraphicUsage,
                MaxDecimalPlaces = 0,
                Maximum = 100,
                MaxLength = 3,
                Minimum = 0
            };

            GraphicUsage.ValueChanged += (s, e) => GraphicUsageChanged(GraphicUsage.Value);

            GraphicAdapterContent.Children.Add(GraphicAdapterText);
            GraphicAdapterContent.Children.Add(GraphicAdapter);

            GraphicUsageContent.Children.Add(GraphicUsageText);
            GraphicUsageContent.Children.Add(GraphicUsage);

            GraphicContent.Children.Add(GraphicAdapterContent);
            GraphicContent.Children.Add(GraphicUsageContent);

            Graphic.FooterCard = GraphicContent;

            Contents.Add(Graphic);

            SPVCEC Memory = new()
            {
                Margin = new Thickness(0, 10, 0, 0)
            };

            Memory.LeftIcon.Symbol = SymbolRegular.Memory16;
            Memory.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Memory");
            Memory.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Memory", "Description");

            ComboBox MemoryPerformance = new();

            MemoryPerformance.SelectionChanged += (s, e) => MemoryPerformanceSelected(MemoryPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                MemoryPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            MemoryPerformance.SelectedIndex = (int)SSDMMB.MemoryPerformance;

            Memory.HeaderFrame = MemoryPerformance;

            StackPanel MemoryContent = new()
            {
                Orientation = Orientation.Horizontal
            };

            TextBlock MemoryUsageText = new()
            {
                Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Memory", "MemoryUsage"),
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.SemiBold
            };

            NumberBox MemoryUsage = new()
            {
                Icon = new SymbolIcon(SymbolRegular.DataUsage24),
                IconPlacement = ElementPlacement.Left,
                Margin = new Thickness(0, 0, 10, 0),
                ClearButtonEnabled = false,
                Value = SMMB.MemoryUsage,
                MaxDecimalPlaces = 0,
                Maximum = 100,
                MaxLength = 3,
                Minimum = 0
            };

            MemoryUsage.ValueChanged += (s, e) => MemoryUsageChanged(MemoryUsage.Value);

            MemoryContent.Children.Add(MemoryUsageText);
            MemoryContent.Children.Add(MemoryUsage);

            Memory.FooterCard = MemoryContent;

            Contents.Add(Memory);

            SPVCEC Network = new()
            {
                Margin = new Thickness(0, 10, 0, 0)
            };

            Network.LeftIcon.Symbol = SymbolRegular.NetworkCheck24;
            Network.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Network");
            Network.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Network", "Description");

            ComboBox NetworkPerformance = new();

            NetworkPerformance.SelectionChanged += (s, e) => NetworkPerformanceSelected(NetworkPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                NetworkPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            NetworkPerformance.SelectedIndex = (int)SSDMMB.NetworkPerformance;

            Network.HeaderFrame = NetworkPerformance;

            StackPanel NetworkContent = new();

            StackPanel NetworkAdapterContent = new()
            {
                Orientation = Orientation.Horizontal
            };

            TextBlock NetworkAdapterText = new()
            {
                Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Network", "NetworkAdapter"),
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.SemiBold
            };

            ComboBox NetworkAdapter = new()
            {
                MaxDropDownHeight = 200,
                MaxWidth = 700
            };

            DynamicScrollViewer.SetVerticalScrollBarVisibility(NetworkAdapter, ScrollBarVisibility.Auto);

            if (SMMS.NetworkInterfaces.Any())
            {
                NetworkAdapter.SelectionChanged += (s, e) => NetworkAdapterSelected($"{NetworkAdapter.SelectedValue}");

                foreach (string Interface in SMMS.NetworkInterfaces)
                {
                    NetworkAdapter.Items.Add(Interface);
                }

                string SelectedAdapter = SMMB.NetworkAdapter;

                if (string.IsNullOrEmpty(SelectedAdapter))
                {
                    NetworkAdapter.SelectedIndex = 0;
                }
                else
                {
                    NetworkAdapter.SelectedValue = SelectedAdapter;
                }
            }
            else
            {
                NetworkAdapter.Items.Add(new ComboBoxItem()
                {
                    Content = SRER.GetValue("Portal", "PerformanceSettingPage", "Network", "NetworkAdapter", "Empty"),
                    IsSelected = true
                });
            }

            StackPanel NetworkUploadContent = new()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            TextBlock NetworkUploadText = new()
            {
                Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Network", "NetworkUpload"),
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.SemiBold
            };

            NumberBox NetworkUpload = new()
            {
                Icon = new SymbolIcon(SymbolRegular.ArrowUpload24),
                IconPlacement = ElementPlacement.Left,
                Margin = new Thickness(0, 0, 10, 0),
                ClearButtonEnabled = false,
                Value = SMMB.UploadValue,
                MaxDecimalPlaces = 0,
                Maximum = 99999999,
                MaxLength = 8,
                Minimum = 0
            };

            NetworkUpload.ValueChanged += (s, e) => NetworkUploadChanged(NetworkUpload.Value);

            ComboBox NetworkUploadType = new()
            {
                MaxDropDownHeight = 200
            };

            DynamicScrollViewer.SetVerticalScrollBarVisibility(NetworkUploadType, ScrollBarVisibility.Auto);

            NetworkUploadType.SelectionChanged += (s, e) => NetworkUploadTypeSelected(NetworkUploadType.SelectedIndex);

            foreach (SEST Type in Enum.GetValues(typeof(SEST)))
            {
                NetworkUploadType.Items.Add(Type);
            }

            NetworkUploadType.SelectedIndex = (int)SMMB.UploadType;

            StackPanel NetworkDownloadContent = new()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            TextBlock NetworkDownloadText = new()
            {
                Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Network", "NetworkDownload"),
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.SemiBold
            };

            NumberBox NetworkDownload = new()
            {
                Icon = new SymbolIcon(SymbolRegular.ArrowDownload24),
                IconPlacement = ElementPlacement.Left,
                Margin = new Thickness(0, 0, 10, 0),
                ClearButtonEnabled = false,
                Value = SMMB.DownloadValue,
                MaxDecimalPlaces = 0,
                Maximum = 99999999,
                MaxLength = 8,
                Minimum = 0
            };

            NetworkDownload.ValueChanged += (s, e) => NetworkDownloadChanged(NetworkDownload.Value);

            ComboBox NetworkDownloadType = new()
            {
                MaxDropDownHeight = 200
            };

            DynamicScrollViewer.SetVerticalScrollBarVisibility(NetworkDownloadType, ScrollBarVisibility.Auto);

            NetworkDownloadType.SelectionChanged += (s, e) => NetworkDownloadTypeSelected(NetworkDownloadType.SelectedIndex);

            foreach (SEST Type in Enum.GetValues(typeof(SEST)))
            {
                NetworkDownloadType.Items.Add(Type);
            }

            NetworkDownloadType.SelectedIndex = (int)SMMB.DownloadType;

            StackPanel NetworkPingContent = new()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            TextBlock NetworkPingText = new()
            {
                Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Network", "NetworkPing"),
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.SemiBold
            };

            NumberBox NetworkPing = new()
            {
                Icon = new SymbolIcon(SymbolRegular.HeartPulse24),
                IconPlacement = ElementPlacement.Left,
                Margin = new Thickness(0, 0, 10, 0),
                ClearButtonEnabled = false,
                Value = SMMB.PingValue,
                MaxDecimalPlaces = 0,
                Maximum = 1000,
                MaxLength = 4,
                Minimum = 0
            };

            NetworkPing.ValueChanged += (s, e) => NetworkPingChanged(NetworkPing.Value);

            ComboBox NetworkPingType = new()
            {
                MaxDropDownHeight = 200
            };

            DynamicScrollViewer.SetVerticalScrollBarVisibility(NetworkPingType, ScrollBarVisibility.Auto);

            NetworkPingType.SelectionChanged += (s, e) => NetworkPingTypeSelected($"{NetworkPingType.SelectedValue}");

            foreach (SSDSH Host in SSSHN.GetHost())
            {
                NetworkPingType.Items.Add(Host.Name);
            }

            NetworkPingType.SelectedValue = SMMB.PingType;

            NetworkAdapterContent.Children.Add(NetworkAdapterText);
            NetworkAdapterContent.Children.Add(NetworkAdapter);

            NetworkUploadContent.Children.Add(NetworkUploadText);
            NetworkUploadContent.Children.Add(NetworkUpload);
            NetworkUploadContent.Children.Add(NetworkUploadType);

            NetworkDownloadContent.Children.Add(NetworkDownloadText);
            NetworkDownloadContent.Children.Add(NetworkDownload);
            NetworkDownloadContent.Children.Add(NetworkDownloadType);

            NetworkPingContent.Children.Add(NetworkPingText);
            NetworkPingContent.Children.Add(NetworkPing);
            NetworkPingContent.Children.Add(NetworkPingType);

            NetworkContent.Children.Add(NetworkAdapterContent);
            NetworkContent.Children.Add(NetworkUploadContent);
            NetworkContent.Children.Add(NetworkDownloadContent);
            NetworkContent.Children.Add(NetworkPingContent);

            Network.FooterCard = NetworkContent;

            Contents.Add(Network);

            TextBlock OperatingSystemArea = new()
            {
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                Text = SRER.GetValue("Portal", "Area", "OperatingSystem"),
                Margin = new Thickness(0, 10, 0, 0),
                FontWeight = FontWeights.Bold
            };

            Contents.Add(OperatingSystemArea);

            SPVCEC Lock = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            Lock.LeftIcon.Symbol = SymbolRegular.PersonLock24;
            Lock.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Lock");
            Lock.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Lock", "Description");

            ComboBox LockPerformance = new();

            LockPerformance.SelectionChanged += (s, e) => LockPerformanceSelected(LockPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                LockPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            LockPerformance.SelectedIndex = (int)SSDMMB.LockPerformance;

            Lock.HeaderFrame = LockPerformance;

            Contents.Add(Lock);

            SPVCEC Sleep = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            Sleep.LeftIcon.Symbol = SymbolRegular.Snooze24;
            Sleep.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Sleep");
            Sleep.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Sleep", "Description");

            ComboBox SleepPerformance = new();

            SleepPerformance.SelectionChanged += (s, e) => SleepPerformanceSelected(SleepPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                SleepPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            SleepPerformance.SelectedIndex = (int)SSDMMB.SleepPerformance;

            Sleep.HeaderFrame = SleepPerformance;

            Contents.Add(Sleep);

            SPVCEC Console = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            Console.LeftIcon.Symbol = SymbolRegular.PeopleSwap24;
            Console.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Console");
            Console.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Console", "Description");

            ComboBox ConsolePerformance = new();

            ConsolePerformance.SelectionChanged += (s, e) => ConsolePerformanceSelected(ConsolePerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                ConsolePerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            ConsolePerformance.SelectedIndex = (int)SSDMMB.ConsolePerformance;

            Console.HeaderFrame = ConsolePerformance;

            Contents.Add(Console);

            SPVCEC Session = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            Session.LeftIcon.Symbol = SymbolRegular.PersonDelete24;
            Session.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Session");
            Session.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Session", "Description");

            ComboBox SessionPerformance = new();

            SessionPerformance.SelectionChanged += (s, e) => SessionPerformanceSelected(SessionPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                SessionPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            SessionPerformance.SelectedIndex = (int)SSDMMB.SessionPerformance;

            Session.HeaderFrame = SessionPerformance;

            Contents.Add(Session);

            SPVCEC ScreenSaver = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            ScreenSaver.LeftIcon.Symbol = SymbolRegular.ViewDesktop24;
            ScreenSaver.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "ScreenSaver");
            ScreenSaver.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "ScreenSaver", "Description");

            ComboBox ScreenSaverPerformance = new();

            ScreenSaverPerformance.SelectionChanged += (s, e) => ScreenSaverPerformanceSelected(ScreenSaverPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                ScreenSaverPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            ScreenSaverPerformance.SelectedIndex = (int)SSDMMB.ScreenSaverPerformance;

            ScreenSaver.HeaderFrame = ScreenSaverPerformance;

            Contents.Add(ScreenSaver);

            TextBlock LaptopArea = new()
            {
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                Text = SRER.GetValue("Portal", "Area", "Laptop"),
                Margin = new Thickness(0, 10, 0, 0),
                FontWeight = FontWeights.Bold
            };

            Contents.Add(LaptopArea);

            SPVCEC Battery = new()
            {
                Margin = new Thickness(0, 10, 0, 0)
            };

            Battery.LeftIcon.Symbol = BatterySymbol(SMMB.BatteryUsage);
            Battery.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Battery");
            Battery.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Battery", "Description");

            ComboBox BatteryPerformance = new();

            BatteryPerformance.SelectionChanged += (s, e) => BatteryPerformanceSelected(BatteryPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                BatteryPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            BatteryPerformance.SelectedIndex = (int)SSDMMB.BatteryPerformance;

            Battery.HeaderFrame = BatteryPerformance;

            StackPanel BatteryContent = new()
            {
                Orientation = Orientation.Horizontal
            };

            TextBlock BatteryUsageText = new()
            {
                Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Battery", "BatteryUsage"),
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.SemiBold
            };

            NumberBox BatteryUsage = new()
            {
                Icon = new SymbolIcon(SymbolRegular.BatteryWarning24),
                IconPlacement = ElementPlacement.Left,
                Margin = new Thickness(0, 0, 10, 0),
                ClearButtonEnabled = false,
                Value = SMMB.BatteryUsage,
                MaxDecimalPlaces = 0,
                Maximum = 100,
                MaxLength = 3,
                Minimum = 0
            };

            BatteryUsage.ValueChanged += (s, e) => BatteryUsageChanged(Battery, BatteryUsage.Value);

            BatteryContent.Children.Add(BatteryUsageText);
            BatteryContent.Children.Add(BatteryUsage);

            Battery.FooterCard = BatteryContent;

            Contents.Add(Battery);

            SPVCEC BatterySaver = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            BatterySaver.LeftIcon.Symbol = SymbolRegular.BatterySaver24;
            BatterySaver.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "BatterySaver");
            BatterySaver.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "BatterySaver", "Description");

            ComboBox BatterySaverPerformance = new();

            BatterySaverPerformance.SelectionChanged += (s, e) => BatterySaverPerformanceSelected(BatterySaverPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                BatterySaverPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            BatterySaverPerformance.SelectedIndex = (int)SSDMMB.BatterySaverPerformance;

            BatterySaver.HeaderFrame = BatterySaverPerformance;

            Contents.Add(BatterySaver);

            TextBlock SystemArea = new()
            {
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                Text = SRER.GetValue("Portal", "Area", "System"),
                Margin = new Thickness(0, 10, 0, 0),
                FontWeight = FontWeights.Bold
            };

            Contents.Add(SystemArea);

            SPVCEC Virtual = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            Virtual.LeftIcon.Symbol = SymbolRegular.DesktopCheckmark24;
            Virtual.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Virtual");
            Virtual.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Virtual", "Description");

            ComboBox VirtualPerformance = new();

            VirtualPerformance.SelectionChanged += (s, e) => VirtualPerformanceSelected(VirtualPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                VirtualPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            VirtualPerformance.SelectedIndex = (int)SSDMMB.VirtualPerformance;

            Virtual.HeaderFrame = VirtualPerformance;

            Contents.Add(Virtual);

            SPVCEC Remote = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            Remote.LeftIcon.Symbol = SymbolRegular.Remote20;
            Remote.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Remote");
            Remote.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Remote", "Description");

            ComboBox RemotePerformance = new();

            RemotePerformance.SelectionChanged += (s, e) => RemotePerformanceSelected(RemotePerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                RemotePerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            RemotePerformance.SelectedIndex = (int)SSDMMB.RemotePerformance;

            Remote.HeaderFrame = RemotePerformance;

            Contents.Add(Remote);

            SPVCEC FullScreen = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            FullScreen.LeftIcon.Symbol = SymbolRegular.FullScreenMaximize24;
            FullScreen.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "FullScreen");
            FullScreen.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "FullScreen", "Description");

            ComboBox FullScreenPerformance = new();

            FullScreenPerformance.SelectionChanged += (s, e) => FullScreenPerformanceSelected(FullScreenPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                FullScreenPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            FullScreenPerformance.SelectedIndex = (int)SSDMMB.FullScreenPerformance;

            FullScreen.HeaderFrame = FullScreenPerformance;

            Contents.Add(FullScreen);

            SPVCEC Focus = new()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Expandable = false
            };

            Focus.LeftIcon.Symbol = SymbolRegular.CursorHover24;
            Focus.Title.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Focus");
            Focus.Description.Text = SRER.GetValue("Portal", "PerformanceSettingPage", "Focus", "Description");

            ComboBox FocusPerformance = new();

            FocusPerformance.SelectionChanged += (s, e) => FocusPerformanceSelected(FocusPerformance.SelectedIndex);

            foreach (SSDEPT Type in Enum.GetValues(typeof(SSDEPT)))
            {
                FocusPerformance.Items.Add(SRER.GetValue("Portal", "Enum", "PerformanceType", $"{Type}"));
            }

            FocusPerformance.SelectedIndex = (int)SSDMMB.FocusPerformance;

            Focus.HeaderFrame = FocusPerformance;

            Contents.Add(Focus);

            _isInitialized = true;
        }

        private SymbolRegular BatterySymbol(int Value)
        {
            if (Value <= 0)
            {
                return SymbolRegular.Battery024;
            }
            else if (Value <= 10)
            {
                return SymbolRegular.Battery124;
            }
            else if (Value <= 20)
            {
                return SymbolRegular.Battery224;
            }
            else if (Value <= 30)
            {
                return SymbolRegular.Battery324;
            }
            else if (Value <= 40)
            {
                return SymbolRegular.Battery424;
            }
            else if (Value <= 50)
            {
                return SymbolRegular.Battery524;
            }
            else if (Value <= 60)
            {
                return SymbolRegular.Battery624;
            }
            else if (Value <= 70)
            {
                return SymbolRegular.Battery724;
            }
            else if (Value <= 80)
            {
                return SymbolRegular.Battery824;
            }
            else if (Value <= 90)
            {
                return SymbolRegular.Battery924;
            }
            else
            {
                return SymbolRegular.Battery1024;
            }
        }

        private void MemoryUsageChanged(double? Value)
        {
            int NewValue = Convert.ToInt32(Value);

            if (NewValue != SMMB.MemoryUsage)
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.MemoryUsage, NewValue);
            }
        }

        private void NetworkPingChanged(double? Value)
        {
            int NewValue = Convert.ToInt32(Value);

            if (NewValue != SMMB.PingValue)
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.PingValue, NewValue);
            }
        }

        private void GraphicUsageChanged(double? Value)
        {
            int NewValue = Convert.ToInt32(Value);

            if (NewValue != SMMB.GraphicUsage)
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.GraphicUsage, NewValue);
            }
        }

        private void LockPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.LockPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.LockPerformance, Type);
            }
        }

        private void FocusPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.FocusPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.FocusPerformance, Type);
            }
        }

        private void NetworkUploadChanged(double? Value)
        {
            int NewValue = Convert.ToInt32(Value);

            if (NewValue != SMMB.UploadValue)
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.UploadValue, NewValue);
            }
        }

        private void SleepPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.SleepPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.SleepPerformance, Type);
            }
        }

        private void CommunicationTypeSelected(int Index)
        {
            if (Index != (int)SSDMMB.CommunicationType)
            {
                SSDECNT Type = (SSDECNT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.CommunicationType, Type);
            }
        }

        private void GraphicAdapterSelected(string Value)
        {
            if (Value != SMMB.GraphicAdapter)
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.GraphicAdapter, Value);
            }
        }

        private void MemoryPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.MemoryPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.MemoryPerformance, Type);
            }
        }

        private void NetworkAdapterSelected(string Value)
        {
            if (Value != SMMB.NetworkAdapter)
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.NetworkAdapter, Value);
            }
        }

        private void NetworkUploadTypeSelected(int Index)
        {
            if (Index != (int)SMMB.UploadType)
            {
                SEST Type = (SEST)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.UploadType, Type);
            }
        }

        private void ProcessorUsageChanged(double? Value)
        {
            int NewValue = Convert.ToInt32(Value);

            if (NewValue != SMMB.ProcessorUsage)
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.ProcessorUsage, NewValue);
            }
        }

        private void RemotePerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.RemotePerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.RemotePerformance, Type);
            }
        }

        private void BatteryPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.BatteryPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.BatteryPerformance, Type);
            }
        }

        private void ConsolePerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.ConsolePerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.ConsolePerformance, Type);
            }
        }

        private void GraphicPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.GraphicPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.GraphicPerformance, Type);
            }
        }

        private void NetworkDownloadChanged(double? Value)
        {
            int NewValue = Convert.ToInt32(Value);

            if (NewValue != SMMB.DownloadValue)
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.DownloadValue, NewValue);
            }
        }

        private void NetworkPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.NetworkPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.NetworkPerformance, Type);
            }
        }

        private void NetworkPingTypeSelected(string Value)
        {
            if (Value != SMMB.PingType)
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.PingType, Value);
            }
        }

        private void SessionPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.SessionPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.SessionPerformance, Type);
            }
        }

        private void VirtualPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.VirtualPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.VirtualPerformance, Type);
            }
        }

        private void NetworkDownloadTypeSelected(int Index)
        {
            if (Index != (int)SMMB.DownloadType)
            {
                SEST Type = (SEST)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.DownloadType, Type);
            }
        }

        private void PausePerformanceTypeSelected(int Index)
        {
            if (Index != (int)SSDMMB.PausePerformanceType)
            {
                SSDEPPT Type = (SSDEPPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.PausePerformanceType, Type);
            }
        }

        private void ProcessorPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.ProcessorPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.ProcessorPerformance, Type);
            }
        }

        private void FullScreenPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.FullScreenPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.FullScreenPerformance, Type);
            }
        }

        private void ScreenSaverPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.ScreenSaverPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.ScreenSaverPerformance, Type);
            }
        }

        private void BatterySaverPerformanceSelected(int Index)
        {
            if (Index != (int)SSDMMB.BatterySaverPerformance)
            {
                SSDEPT Type = (SSDEPT)Index;

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.BatterySaverPerformance, Type);
            }
        }

        private void CounterStateChecked(SPVCEC Battery, bool State)
        {
            Battery.IsExpand = !State;
            Battery.Expandable = !State;

            SMMI.BackgroundogSettingManager.SetSetting(SMMCB.PerformanceCounter, State);

            if (State)
            {
                SSSHP.Run(SSSMI.Commandog, $"{SMMRG.StartCommand}{SSDECDT.Backgroundog}{SMMRG.ValueSeparator}{SSSMI.Backgroundog}");
            }
            else
            {
                if (SSSHP.Work(SMMRA.Backgroundog))
                {
                    SSSHP.Kill(SMMRA.Backgroundog);
                }
            }
        }

        private void BatteryUsageChanged(SPVCEC Battery, double? Value)
        {
            int NewValue = Convert.ToInt32(Value);

            if (NewValue != SMMB.BatteryUsage)
            {
                Battery.LeftIcon.Symbol = BatterySymbol(NewValue);

                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.BatteryUsage, NewValue);
            }
        }

        public void Dispose()
        {
            Contents.Clear();

            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
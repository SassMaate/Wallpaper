using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
using SMMG = Sucrose.Manager.Manage.General;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SPEIL = Sucrose.Portal.Extension.ImageLoader;
using SPMI = Sucrose.Portal.Manage.Internal;
using SRER = Sucrose.Resources.Extension.Resources;
using SRHR = Sucrose.Resources.Helper.Resources;
using SSDEWT = Sucrose.Shared.Dependency.Enum.WallpaperType;
using SSSHT = Sucrose.Shared.Space.Helper.Tags;
using SSSHV = Sucrose.Shared.Space.Helper.Versionly;
using SSTCLC = Sucrose.Shared.Theme.Converter.LocalizationConverter;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;
using SSTHL = Sucrose.Shared.Theme.Helper.Localization;
using SSTHV = Sucrose.Shared.Theme.Helper.Various;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;
using TextBlock = Wpf.Ui.Controls.TextBlock;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Sucrose.Portal.Views.Controls
{
    /// <summary>
    /// ThemeEdit.xaml etkileşim mantığı
    /// </summary>
    public partial class ThemeEdit : ContentDialog, IDisposable
    {
        private readonly SPEIL Loader = new();
        internal string Theme = string.Empty;
        private SSTHI InfoReserve = new();
        internal SSTHI Info = new();

        public ThemeEdit() : base(SPMI.ContentDialogService.GetDialogHostEx())
        {
            InitializeComponent();
        }

        private string GetSelectedLanguage()
        {
            if (LocalizationComboBox.SelectedItem is ComboBoxItem SelectedItem)
            {
                return SelectedItem.Tag?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        private void OnLanguageChanged(string Code)
        {
            ThemeTitle.IsReadOnly = true;
            ThemeDescription.IsReadOnly = true;

            (ThemeTitle.Text, ThemeDescription.Text) = SSTCLC.Convert(Info, Code);

            ThemeTitle.IsReadOnly = false;
            ThemeDescription.IsReadOnly = false;
        }

        private void PopulateLocalizationComboBox()
        {
            LocalizationComboBox.Items.Clear();

            foreach (string Code in SRHR.ListLanguageManipulated())
            {
                string Language = SRER.GetValue("Locale", Code);
                SymbolRegular Symbol = GetSymbolForLanguageStatus(Code);

                if (SRER.CheckBack("Locale", Code))
                {
                    Language = SRER.GetValue("Portal", "ThemeEdit", "Localization");
                }
                else
                {
                    Language = Regex.Replace(Language, @"\s*\(.*?\)", "");
                }

                ComboBoxItem Item = CreateComboBoxItem(Code, Language, Symbol);

                if (Item.IsSelected)
                {
                    (ThemeTitle.Text, ThemeDescription.Text) = SSTCLC.Convert(Info, Code);
                }

                LocalizationComboBox.Items.Add(Item);
            }
        }

        private void SetSelectedLanguage(string Code)
        {
            for (int Index = 0; Index < LocalizationComboBox.Items.Count; Index++)
            {
                if (LocalizationComboBox.Items[Index] is ComboBoxItem Item && Item.Tag?.ToString() == Code)
                {
                    LocalizationComboBox.SelectedIndex = Index;
                    break;
                }
            }
        }

        private SymbolRegular GetSymbolForLanguageStatus(string Code)
        {
            (string Title, string Description) = SSTCLC.Convert(Info, Code);

            if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Description))
            {
                if (Info.Localization != null && Info.Localization.ContainsKey(Code))
                {
                    Info.Localization.Remove(Code);

                    if (!Info.Localization.Any())
                    {
                        Info.Localization = null;
                    }
                }

                return SymbolRegular.Dismiss48;
            }
            else if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description))
            {
                return SymbolRegular.Prohibited48;
            }
            else
            {
                (string ReserveTitle, string ReserveDescription) = SSTCLC.Convert(InfoReserve, Code);

                if (ReserveTitle != Title || ReserveDescription != Description)
                {
                    return SymbolRegular.Edit48;
                }
                else
                {
                    return SymbolRegular.Checkmark48;
                }
            }
        }

        private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            InfoReserve = SSTHI.FromJson(JsonConvert.SerializeObject(Info));

            PopulateLocalizationComboBox();

            LocalizationComboBox.SelectionChanged += LocalizationComboBox_SelectionChanged;

            ThemeTitle.IsReadOnly = false;
            ThemeAuthor.Text = Info.Author;
            ThemeContact.Text = Info.Contact;
            ThemeDescription.IsReadOnly = false;
            ThemeArguments.Text = Info.Arguments;
            ThemeTags.Text = SSSHT.Join(Info.Tags, ",", false, string.Empty);

            if (Info.Type != SSDEWT.Application)
            {
                Arguments.Visibility = Visibility.Collapsed;
            }

            string ImagePath = Path.Combine(Theme, Info.Thumbnail);

            if (File.Exists(ImagePath))
            {
                try
                {
                    ThemeThumbnail.Source = Loader.LoadOptimal(ImagePath, true, 600);
                }
                catch (Exception Exception)
                {
                    await SSWEW.Watch_CatchException(Exception);
                }
            }
        }

        private void ThemeTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox ThemeTitle && !ThemeTitle.IsReadOnly)
            {
                string Language = GetSelectedLanguage();

                if (string.IsNullOrEmpty(Language))
                {
                    Info.Title = ThemeTitle.Text;
                }
                else
                {
                    Info.Localization ??= [];

                    if (!Info.Localization.ContainsKey(Language))
                    {
                        Info.Localization[Language] = new SSTHL();
                    }

                    Info.Localization[Language].Title = ThemeTitle.Text;
                }

                if (LocalizationComboBox.SelectedItem is ComboBoxItem Item)
                {
                    Item.Content = CreateComboBoxItem(Language, ((TextBlock)((StackPanel)Item.Content).Children[1]).Text, GetSymbolForLanguageStatus(Language)).Content;
                }
            }
        }

        private void ContentDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Escape) && (ThemeTitle.IsFocused || ThemeAuthor.IsFocused || ThemeContact.IsFocused || ThemeArguments.IsFocused || ThemeDescription.IsFocused))
            {
                e.Handled = true;
            }
        }

        private void ThemeDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox ThemeDescription && !ThemeDescription.IsReadOnly)
            {
                string Language = GetSelectedLanguage();

                if (string.IsNullOrEmpty(Language))
                {
                    Info.Description = ThemeDescription.Text;
                }
                else
                {
                    Info.Localization ??= [];

                    if (!Info.Localization.ContainsKey(Language))
                    {
                        Info.Localization[Language] = new SSTHL();
                    }

                    Info.Localization[Language].Description = ThemeDescription.Text;
                }

                if (LocalizationComboBox.SelectedItem is ComboBoxItem Item)
                {
                    Item.Content = CreateComboBoxItem(Language, ((TextBlock)((StackPanel)Item.Content).Children[1]).Text, GetSymbolForLanguageStatus(Language)).Content;
                }
            }
        }

        private ComboBoxItem CreateComboBoxItem(string Code, string Name, SymbolRegular Symbol)
        {
            StackPanel StackPanel = new()
            {
                FlowDirection = Application.Current.MainWindow.FlowDirection,
                VerticalAlignment = VerticalAlignment.Stretch,
                Orientation = Orientation.Horizontal
            };

            SymbolIcon Icon = new()
            {
                Width = 32,
                Symbol = Symbol
            };

            TextBlock TextBlock = new()
            {
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                TextWrapping = TextWrapping.WrapWithOverflow,
                Margin = new Thickness(10, 0, 10, 0),
                FontSize = 14,
                Text = Name
            };

            StackPanel.Children.Add(Icon);
            StackPanel.Children.Add(TextBlock);

            return new ComboBoxItem
            {
                IsSelected = (SMMG.Culture == Code || string.IsNullOrEmpty(Code)) && Symbol == SymbolRegular.Checkmark48,
                Content = StackPanel,
                Tag = Code
            };
        }

        private void LocalizationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox ComboBox && ComboBox.SelectedItem is ComboBoxItem SelectedItem)
            {
                OnLanguageChanged(SelectedItem.Tag?.ToString() ?? string.Empty);
            }
        }

        protected override void OnButtonClick(ContentDialogButton Button)
        {
            if (Button == ContentDialogButton.Primary)
            {
                foreach (ComboBoxItem Item in LocalizationComboBox.Items)
                {
                    string Code = $"{Item.Tag}";

                    if ((string.IsNullOrEmpty(Code) && GetSymbolForLanguageStatus(Code) is SymbolRegular.Dismiss48 or SymbolRegular.Prohibited48) || GetSymbolForLanguageStatus(Code) == SymbolRegular.Prohibited48)
                    {
                        if (GetSelectedLanguage() != Code)
                        {
                            SetSelectedLanguage(Code);
                        }

                        (string Title, string Description) = SSTCLC.Convert(Info, Code);

                        if (string.IsNullOrEmpty(Title))
                        {
                            ThemeTitle.Focus();
                            return;
                        }
                        else if (string.IsNullOrEmpty(Description))
                        {
                            ThemeDescription.Focus();
                            return;
                        }
                    }
                }


                if (string.IsNullOrEmpty(ThemeAuthor.Text))
                {
                    ThemeAuthor.Focus();
                    return;
                }
                else if (!SSTHV.IsUrl(ThemeContact.Text) && !SSTHV.IsMail(ThemeContact.Text))
                {
                    ThemeContact.Focus();
                    return;
                }
                else
                {
                    if (string.IsNullOrEmpty(ThemeTags.Text))
                    {
                        Info.Tags = null;
                    }
                    else
                    {
                        if (ThemeTags.Text.Contains(','))
                        {
                            Info.Tags = ThemeTags.Text.Split(',').Select(Tag => Tag.TrimStart().TrimEnd()).ToArray();

                            if (Info.Tags.Count() is < 1 or > 5)
                            {
                                ThemeTags.Focus();
                                return;
                            }
                            else if (Info.Tags.Any(Tag => Tag.Length is < 1 or > 20 || string.IsNullOrWhiteSpace(Tag)))
                            {
                                ThemeTags.Focus();
                                return;
                            }
                        }
                        else
                        {
                            if (ThemeTags.Text.Length is < 1 or > 20 || string.IsNullOrWhiteSpace(ThemeTags.Text))
                            {
                                ThemeTags.Focus();
                                return;
                            }
                            else
                            {
                                Info.Tags = new[]
                                {
                                    ThemeTags.Text.TrimStart().TrimEnd()
                                };
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(ThemeArguments.Text))
                    {
                        Info.Arguments = null;
                    }
                    else
                    {
                        if (ThemeArguments.Text.Length is > 250 || string.IsNullOrWhiteSpace(ThemeArguments.Text))
                        {
                            ThemeTags.Focus();
                            return;
                        }
                        else
                        {
                            Info.Arguments = ThemeArguments.Text;
                        }
                    }

                    Info.Author = ThemeAuthor.Text;
                    Info.Contact = ThemeContact.Text;
                    Info.Version = SSSHV.Increment(Info.Version);

                    SSTHI.Write(Path.Combine(Theme, SMMRC.SucroseInfo), Info);
                }
            }

            base.OnButtonClick(Button);
        }

        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
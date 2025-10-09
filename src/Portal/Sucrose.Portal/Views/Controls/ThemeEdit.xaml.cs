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
        internal SSTHI Info = new();

        public ThemeEdit() : base(SPMI.ContentDialogService.GetDialogHost())
        {
            InitializeComponent();
        }

        private void ContentDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Escape) && (ThemeTitle.IsFocused || ThemeAuthor.IsFocused || ThemeContact.IsFocused || ThemeArguments.IsFocused || ThemeDescription.IsFocused))
            {
                e.Handled = true;
            }
        }

        private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize LocalizationComboBox with dynamic items
            PopulateLocalizationComboBox();

            // Add selection changed event handler
            LocalizationComboBox.SelectionChanged += LocalizationComboBox_SelectionChanged;

            ThemeAuthor.Text = Info.Author;
            ThemeContact.Text = Info.Contact;
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

        protected override void OnButtonClick(ContentDialogButton Button)
        {
            if (Button == ContentDialogButton.Primary)
            {
                foreach (ComboBoxItem Item in LocalizationComboBox.Items)
                {
                    string Code = $"{Item.Tag}";

                    if ((string.IsNullOrEmpty(Code) && GetSymbolForLanguageStatus(string.Empty) != SymbolRegular.Checkmark48) || GetSymbolForLanguageStatus(Code) == SymbolRegular.Prohibited48)
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
                        else if (true)
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

        /// <summary>
        /// Populates the LocalizationComboBox with available languages dynamically
        /// </summary>
        private void PopulateLocalizationComboBox()
        {
            LocalizationComboBox.Items.Clear();

            foreach (string Code in SRHR.ListLanguageManipulated())
            {
                string Language = SRER.GetValue("Locale", Code);
                SymbolRegular Symbol = GetSymbolForLanguageStatus(Code);

                if (SRER.CheckBack("Locale", Code))
                {
                    Language = "Varsayılan";
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

        /// <summary>
        /// Creates a ComboBoxItem with icon and text
        /// </summary>
        private ComboBoxItem CreateComboBoxItem(string code, string name, SymbolRegular symbol)
        {
            StackPanel stackPanel = new()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                Orientation = Orientation.Horizontal
            };

            SymbolIcon icon = new()
            {
                Width = 32,
                HorizontalAlignment = HorizontalAlignment.Left,
                Symbol = symbol
            };

            TextBlock textBlock = new()
            {
                Foreground = SRER.GetResource<Brush>("TextFillColorPrimaryBrush"),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                TextWrapping = TextWrapping.WrapWithOverflow,
                Margin = new Thickness(10, 0, 0, 0),
                FontSize = 14,
                Text = name
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(textBlock);

            return new ComboBoxItem
            {
                IsSelected = (SMMG.Culture == code || string.IsNullOrEmpty(code)) && symbol == SymbolRegular.Checkmark48,
                Content = stackPanel,
                Tag = code
            };
        }

        /// <summary>
        /// Returns appropriate symbol based on language status
        /// </summary>
        private SymbolRegular GetSymbolForLanguageStatus(string languageCode)
        {
            (string Title, string Description) = SSTCLC.Convert(Info, languageCode);

            if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Description))
            {
                return SymbolRegular.Dismiss48;
            }
            else if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description))
            {
                return SymbolRegular.Prohibited48;
            }
            else if (ThemeTitle.Text != Info.Title && ThemeDescription.Text != Info.Description && false) // Both changed but not saved
            {
                return SymbolRegular.Edit48;
            }
            else
            {
                return SymbolRegular.Checkmark48;
            }
        }

        /// <summary>
        /// Gets the selected language code from ComboBox
        /// </summary>
        public string GetSelectedLanguage()
        {
            if (LocalizationComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Tag?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the selected language in ComboBox
        /// </summary>
        public void SetSelectedLanguage(string languageCode)
        {
            for (int i = 0; i < LocalizationComboBox.Items.Count; i++)
            {
                if (LocalizationComboBox.Items[i] is ComboBoxItem item && item.Tag?.ToString() == languageCode)
                {
                    LocalizationComboBox.SelectedIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Handles language selection changes
        /// </summary>
        private void LocalizationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedLanguageCode = selectedItem.Tag?.ToString() ?? string.Empty;

                // Here you can implement language-specific logic
                OnLanguageChanged(selectedLanguageCode);
            }
        }

        /// <summary>
        /// Called when language selection changes
        /// </summary>
        private void OnLanguageChanged(string languageCode)
        {
            // Implement your language change logic here
            // For example:
            // - Update theme info based on selected language
            // - Load localized strings
            // - Update UI elements
            (ThemeTitle.Text, ThemeDescription.Text) = SSTCLC.Convert(Info, languageCode);
        }

        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }

        private void ThemeTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox ThemeTitle)
            {
                string Language = GetSelectedLanguage();

                if (string.IsNullOrEmpty(Language))
                {
                    Info.Title = ThemeTitle.Text;
                }
                else
                {
                    Info.Localization ??= new Dictionary<string, SSTHL>();

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

        private void ThemeDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox ThemeDescription)
            {
                string Language = GetSelectedLanguage();

                if (string.IsNullOrEmpty(Language))
                {
                    Info.Description = ThemeDescription.Text;
                }
                else
                {
                    Info.Localization ??= new Dictionary<string, SSTHL>();

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
    }
}
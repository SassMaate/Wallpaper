using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sucrose.Portal.Controls
{
    public class CustomGroupBox : ContentControl
    {
        static CustomGroupBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomGroupBox), new FrameworkPropertyMetadata(typeof(CustomGroupBox)));
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(object), typeof(CustomGroupBox), new PropertyMetadata(null));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(nameof(HeaderBackground), typeof(Brush), typeof(CustomGroupBox), new PropertyMetadata(Brushes.Transparent));

        public Brush HeaderBackground
        {
            get => (Brush)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }
    }
}
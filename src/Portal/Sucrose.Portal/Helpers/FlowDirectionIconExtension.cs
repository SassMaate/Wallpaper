using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace Sucrose.Portal.Helpers
{
    [MarkupExtensionReturnType(typeof(object))]
    public class FlowDirectionIconExtension : MarkupExtension
    {
        public SymbolRegular LTR { get; set; } = SymbolRegular.Empty;
        public SymbolRegular RTL { get; set; } = SymbolRegular.Empty;

        public FlowDirectionIconExtension() { }

        public FlowDirectionIconExtension(SymbolRegular ltr, SymbolRegular rtl)
        {
            LTR = ltr;
            RTL = rtl;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Application.Current == null)
            {
                return new SymbolIcon(LTR);
            }

            Binding binding = new("MainWindow.FlowDirection")
            {
                Source = Application.Current,
                ConverterParameter = $"{LTR}|{RTL}",
                FallbackValue = new SymbolIcon(LTR),
                Converter = new FlowDirectionToIconConverter()
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
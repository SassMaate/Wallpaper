using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace Sucrose.Portal.Helpers
{
    [MarkupExtensionReturnType(typeof(object))]
    public class FlowDirectionSymbolExtension : MarkupExtension
    {
        public SymbolRegular LTR { get; set; } = SymbolRegular.Empty;
        public SymbolRegular RTL { get; set; } = SymbolRegular.Empty;

        public FlowDirectionSymbolExtension() { }

        public FlowDirectionSymbolExtension(SymbolRegular ltr, SymbolRegular rtl)
        {
            LTR = ltr;
            RTL = rtl;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Application.Current == null)
            {
                return LTR;
            }

            Binding binding = new("MainWindow.FlowDirection")
            {
                FallbackValue = LTR,
                Source = Application.Current,
                ConverterParameter = $"{LTR}|{RTL}",
                Converter = new FlowDirectionToSymbolConverter()
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
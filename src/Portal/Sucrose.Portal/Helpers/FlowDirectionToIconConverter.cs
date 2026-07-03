using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace Sucrose.Portal.Helpers
{
    internal class FlowDirectionToIconConverter : IValueConverter
    {
        private static readonly Dictionary<string, (SymbolRegular LTR, SymbolRegular RTL)> _cache = [];

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SymbolRegular LTR = SymbolRegular.ArrowLeft16;
            SymbolRegular RTL = SymbolRegular.ArrowRight16;

            if (parameter is string paramString)
            {
                if (_cache.TryGetValue(paramString, out (SymbolRegular LTR, SymbolRegular RTL) cachedSymbols))
                {
                    LTR = cachedSymbols.LTR;
                    RTL = cachedSymbols.RTL;
                }
                else
                {
                    string[] parts = paramString.Split('|');

                    if (parts.Length == 2)
                    {
                        if (Enum.TryParse(parts[0], out SymbolRegular parsedLTR))
                        {
                            LTR = parsedLTR;
                        }

                        if (Enum.TryParse(parts[1], out SymbolRegular parsedRTL))
                        {
                            RTL = parsedRTL;
                        }
                    }

                    _cache[paramString] = (LTR, RTL);
                }
            }

            if (value is FlowDirection direction)
            {
                SymbolRegular result = direction == FlowDirection.RightToLeft ? RTL : LTR;

                return new SymbolIcon { Symbol = result };
            }

            return new SymbolIcon(LTR);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
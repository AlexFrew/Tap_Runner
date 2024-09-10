using OpenTap;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TAPRunnerLib
{
    public class VerdictToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string verdict)
            {
                switch (verdict.ToLower())
                {
                    case "pass":
                        return Brushes.Green;

                    case "inconclusive":
                        return Brushes.Yellow;

                    case "fail":
                        return Brushes.Red;

                    case "aborted":
                        return Brushes.Orange;

                    case "error":
                        return Brushes.Purple;

                    case "notset":
                    default:
                        return Brushes.Gray;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WinDbgEx.UICore.Converters {
    public sealed class IconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return value != null ? new Image {
				Source = new BitmapImage(new Uri(value.ToString(), UriKind.RelativeOrAbsolute)),
				Width = 20,
				Height = 20
			} : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}

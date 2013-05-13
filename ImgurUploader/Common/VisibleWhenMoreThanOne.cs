using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ImgurUploader.Common
{
    public sealed class VisibleWhenMoreThanOne : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int)
            {
                int v = (int) value;

                return v > 1 ? Visibility.Visible : Visibility.Collapsed;

            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

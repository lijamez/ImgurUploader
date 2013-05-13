using System;
using Windows.UI.Xaml.Data;

namespace ImgurUploader.Common
{
    public sealed class StringEqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return String.Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ImgurUploader.Common
{
    /// <summary>
    /// Value converter that translates true to false and vice versa.
    /// </summary>
    public sealed class VisibleWhenNonNull : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool inverted = (parameter is string) && String.Equals("Inverted", parameter);

            if (((value is string && !String.IsNullOrEmpty(value as string)) || value != null) ^ inverted)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

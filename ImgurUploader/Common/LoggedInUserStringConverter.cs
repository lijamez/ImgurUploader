using System;
using Windows.UI.Xaml.Data;

namespace ImgurUploader.Common
{
    public sealed class LoggedInUserStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string user = value as string;
            if (!String.IsNullOrEmpty(user))
            {
                return String.Format("Logged in as {0}", user);
            }

            return "Not logged in.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

using ImgurUploader.UploadResult;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ImgurUploader.Common
{
    public sealed class VisibleWhenUploadSuccessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BatchUploadResult r = value as BatchUploadResult;
            if (r == null) return null;

            return BatchUploadResult.GetStatus(r) == BatchUploadResult.Status.SUCCESSFUL ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

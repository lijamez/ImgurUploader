using ImgurUploader.Model;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ImgurUploader.Common
{
    public sealed class VisibleWhenAlbumCreationSucceeded : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Basic<AlbumCreateData> albumCreateData = value as Basic<AlbumCreateData>;
            Visibility visibility = Visibility.Collapsed;

            if (albumCreateData != null)
            {
                try
                {
                    if (albumCreateData.Success)
                    {
                        visibility = Visibility.Visible;
                    }
                }
                catch (Exception) { }
                
            }

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }
    }
}

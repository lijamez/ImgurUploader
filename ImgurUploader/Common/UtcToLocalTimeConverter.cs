﻿using System;
using Windows.UI.Xaml.Data;

namespace ImgurUploader.Common
{
    public sealed class UtcToLocalTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime)
            {
                return ((DateTime) value).ToLocalTime();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

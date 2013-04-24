using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ImgurUploader.Common
{
    
    public sealed class EnabledWithSelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int && parameter is string)
            {
                int v = (int) value;

                if (String.Equals(parameter, "None"))
                {
                    return v == 0;
                }
                else if (String.Equals(parameter, "One"))
                {
                    return v == 1;
                }
                else if (String.Equals(parameter, "OneOrMore"))
                {
                    return v >= 1;
                }
                else if (String.Equals(parameter, "MoreThanOne"))
                {
                    return v > 1;
                }

                return false;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

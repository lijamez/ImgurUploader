using ImgurUploader.UploadResult;
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
    
    public sealed class UploadStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is BatchUploadResult)
            {
                BatchUploadResult result = (BatchUploadResult)value;

                switch (BatchUploadResult.GetStatus(result))
                {
                    case BatchUploadResult.Status.SUCCESSFUL:
                        return "Successful";
                    case BatchUploadResult.Status.PARTIAL:
                        return "Partial";
                    case BatchUploadResult.Status.FAILED:
                        return "Failed";
                    case BatchUploadResult.Status.INVALID:
                        return "Invalid Data";
                    default:
                        return null;
                }
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

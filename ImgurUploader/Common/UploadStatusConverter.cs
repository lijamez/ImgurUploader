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
            if (value is FinishedUploadResult)
            {
                FinishedUploadResult result = (FinishedUploadResult)value;

                if (result.AlbumCreateResults == null || (result.AlbumCreateResults != null && result.AlbumCreateResults.Success))
                {
                    if (result.Images.FailedUploads.Count == 0)
                    {
                        return "Successful";
                    }
                    else if (result.Images.SuccessfulUploads.Count == 0)
                    {
                        return "Failed";
                    }
                    else
                    {
                        return "Partial";
                    }
                }
                else
                {
                    return "Failed";
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

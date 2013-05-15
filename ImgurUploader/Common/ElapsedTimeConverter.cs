using ImgurUploader.UploadResult;
using System;
using System.Text;
using Windows.UI.Xaml.Data;

namespace ImgurUploader.Common
{
    public sealed class ElapsedTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BatchUploadResult result = value as BatchUploadResult;
            if (result != null)
            {
                if (result.FinishDate < result.StartDate || result.FinishDate == default(DateTime) || result.StartDate == default(DateTime))
                {
                    return String.Empty;
                }

                TimeSpan duration = result.FinishDate.Subtract(result.StartDate);

                double timeInMs = duration.TotalMilliseconds; 

                int days = (int) timeInMs/ 86400000;
                timeInMs -= 86400000 * days;

                int hours = (int)timeInMs / 3600000;
                timeInMs -= 3600000 * hours;

                int minutes = (int)timeInMs / 60000;
                timeInMs -= 60000 * minutes;

                int seconds = (int)timeInMs / 1000;

                bool start = false;
                StringBuilder sb = new StringBuilder();

                if (days > 0 || start)
                {
                    if (start) sb.Append(" ");
                    if (days == 1)
                        sb.Append(String.Format("{0} day", days));
                    else
                        sb.Append(String.Format("{0} days", days));
                    start = true;
                }

                if (hours > 0 || start)
                {
                    if (start) sb.Append(" ");
                    if (hours == 1)
                        sb.Append(String.Format("{0} hour", hours));
                    else
                        sb.Append(String.Format("{0} hours", hours));
                    start = true;
                }

                if (minutes > 0 || start)
                {
                    if (start) sb.Append(" ");

                    if (minutes == 1)
                        sb.Append(String.Format("{0} minute", minutes));
                    else
                        sb.Append(String.Format("{0} minutes", minutes));

                    start = true;
                }

                if (seconds > 0 || start)
                {
                    if (start) sb.Append(" ");

                    if (seconds == 1)
                        sb.Append(String.Format(" {0} second", seconds));
                    else
                        sb.Append(String.Format(" {0} seconds", seconds));
                    start = true;
                }

                return sb.ToString();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}

using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ImgurUploader.QuickButtons
{
    public sealed partial class SubmitToRedditButton : UserControl
    {
        public SubmitToRedditButton()
        {
            this.InitializeComponent();
        }

        private async void ShareToRedditButton_Click(object sender, RoutedEventArgs e)
        {
            string url = BatchUploadResult.GetShareableUrl(this.DataContext as BatchUploadResult);

            if (url != null)
            {
                await Launcher.LaunchUriAsync(new Uri(String.Format("{0}{1}", "http://www.reddit.com/submit?url=", url)));
            }
        }
    }
}

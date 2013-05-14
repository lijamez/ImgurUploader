using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
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
    public sealed partial class CopyLinkButton : UserControl
    {
        public CopyLinkButton()
        {
            this.InitializeComponent();
        }


        private void CpLinkButton_Click(object sender, RoutedEventArgs e)
        {
            string url = FinishedUploadResult.GetShareableUrl(this.DataContext as FinishedUploadResult);

            if (url != null)
            {
                DataPackage dpkg = new DataPackage();
                dpkg.SetText(url);
                Clipboard.SetContent(dpkg);
            }

        }
    }
}

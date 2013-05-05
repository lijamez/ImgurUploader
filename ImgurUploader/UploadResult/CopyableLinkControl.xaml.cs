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

namespace ImgurUploader.UploadResult
{
    public sealed partial class CopyableLinkControl : UserControl
    {
        public string Title
        {
            get
            {
                return TitleTextBlock.Text;
            }
            set
            {
                TitleTextBlock.Text = value;
            }
        }

        public string Value
        {
            get
            {
                return ContentTextBox.Text;
            }
            set
            {
                ContentTextBox.Text = value;
            }
        }


        public bool IsViewableInBrowser
        {
            get
            {
                return ViewInBrowserButton.Visibility == Windows.UI.Xaml.Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    ViewInBrowserButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    ViewInBrowserButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        public Visibility ViewInBrowserButtonVisibility
        {
            get
            {
                return IsViewableInBrowser ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public CopyableLinkControl(string title, string copyableValue, bool isViewableInBrowser)
        {
            this.InitializeComponent();
            this.DataContext = this;

            Title = title;
            Value = copyableValue;
            IsViewableInBrowser = isViewableInBrowser;
        }

        public CopyableLinkControl()
        {
            this.InitializeComponent();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dpkg = new DataPackage();
            dpkg.SetText(ContentTextBox.Text);
            Clipboard.SetContent(dpkg);            
        }

        private async void ViewInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(ContentTextBox.Text));
        }

        private void ContentTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ContentTextBox.SelectAll();
        }
    }
}

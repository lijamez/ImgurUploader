using ImgurUploader.Common;
using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ImgurUploader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadResultPage : LayoutAwarePage
    {
        public UploadResultPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            object arg = e.Parameter;

            ResultGrid.Children.Clear();

            UploadResultsControl resultsControl = null;

            if (arg is FinishedUploadResult)
            {
                resultsControl = new UploadResultsControl(arg as FinishedUploadResult);
            }
            else if (arg is string)
            {
                try
                {
                    int uploadHistoryIndex = int.Parse(arg as string);
                    resultsControl = new UploadResultsControl(App.UploadHistory[uploadHistoryIndex]);
                }
                catch (Exception) { }
            }

            if (resultsControl != null)
            {
                ResultGrid.Children.Add(resultsControl);
            }
            else
            {
                this.Frame.GoBack();
            }

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}

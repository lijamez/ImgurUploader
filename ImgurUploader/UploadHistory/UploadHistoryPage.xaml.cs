using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ImgurUploader.UploadHistory
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadHistoryPage : Page
    {
        ObservableCollection<FinishedUploadResult> _dataSource;

        public UploadHistoryPage()
        {
            this.InitializeComponent();
            _dataSource = App.UploadHistoryMgr.UploadHistory;

            HistoryListView.DataContext = _dataSource;
            if (HistoryListView.Items.Count > 0)
            {
                HistoryListView.SelectedIndex = 0;
            }

        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView list = sender as ListView;
            if (list != null)
            {
                FinishedUploadResult result = list.SelectedItem as FinishedUploadResult;
                if (result != null)
                {
                    UploadResults.Result = result;
                }
            }
        }

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog confirmationDialog = new MessageDialog("Are you sure you want to clear history?");
            confirmationDialog.Commands.Add(new UICommand("Yes", (command) =>
                {
                    _dataSource.Clear();
                }));

            confirmationDialog.Commands.Add(new UICommand("Nope"));
            confirmationDialog.DefaultCommandIndex = 1;

            await confirmationDialog.ShowAsync();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _dataSource.Remove(HistoryListView.SelectedItem as FinishedUploadResult);
        }

        private async void BatchListView_ItemClicked(object sender, ItemClickEventArgs e)
        {
            UploadImageResult imageResult = e.ClickedItem as UploadImageResult;
            if (imageResult != null && imageResult.Result.Success)
            {
                await Launcher.LaunchUriAsync(new Uri(imageResult.Result.Data.Link));
            }
        }


    }
}

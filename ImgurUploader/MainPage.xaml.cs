using ImgurUploader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ImgurUploader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ImgurAPI _api = new ImgurAPI();
        private FileOpenPicker _filePicker;
        private FileOpenPicker FilePicker
        {
            get
            {
                if (_filePicker == null)
                {
                    FileOpenPicker picker = new FileOpenPicker();
                    picker.ViewMode = PickerViewMode.Thumbnail;

                    // http://imgur.com/help/uploading#allowedtoupload
                    picker.FileTypeFilter.Add(".jpg");
                    picker.FileTypeFilter.Add(".jpeg");
                    picker.FileTypeFilter.Add(".png");
                    picker.FileTypeFilter.Add(".gif");
                    picker.FileTypeFilter.Add(".apng");
                    picker.FileTypeFilter.Add(".tiff");
                    picker.FileTypeFilter.Add(".tif");
                    picker.FileTypeFilter.Add(".bmp");
                    picker.FileTypeFilter.Add(".pdf");
                    picker.FileTypeFilter.Add(".xcf");

                    _filePicker = picker;
                }

                return _filePicker;
            }
        }
        private StorageFile SelectedFile
        {
            get;
            set;
        }
        private double _settingsWidth = 346;
        private Popup _settingsPopup;

        public MainPage()
        {
            this.InitializeComponent();
        }


        /// <summary>
        /// When the Popup closes we no longer need to monitor activation changes.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        void OnPopupClosed(object sender, object e)
        {
            Window.Current.Activated -= OnWindowActivated;
        }

        /// <summary>
        /// We use the window's activated event to force closing the Popup since a user maybe interacted with
        /// something that didn't normally trigger an obvious dismiss.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                _settingsPopup.IsOpen = false;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            AddSettings();

        }

        /// <summary>
        /// This the event handler for the "Defaults" button added to the settings charm. This method
        /// is responsible for creating the Popup window will use as the container for our settings Flyout.
        /// The reason we use a Popup is that it gives us the "light dismiss" behavior that when a user clicks away 
        /// from our custom UI it just dismisses.  This is a principle in the Settings experience and you see the
        /// same behavior in other experiences like AppBar. 
        /// </summary>
        /// <param name="command"></param>
        void onSettingsCommand(IUICommand command)
        {
            Popup settingsPopup = new Popup();
            settingsPopup.Closed += OnPopupClosed;
            Window.Current.Activated += OnWindowActivated;
            settingsPopup.IsLightDismissEnabled = true;
            settingsPopup.Width = _settingsWidth;
            settingsPopup.Height = Window.Current.Bounds.Height;
            
            // Add the proper animation for the panel.
            settingsPopup.ChildTransitions = new TransitionCollection();
            settingsPopup.ChildTransitions.Add(new PaneThemeTransition()
            {
                Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ?
                       EdgeTransitionLocation.Right :
                       EdgeTransitionLocation.Left
            });
            
            // Create a SettingsFlyout the same dimenssions as the Popup.
            AccountFlyout mypane = new AccountFlyout();
            mypane.Width = _settingsWidth;
            mypane.Height = Window.Current.Bounds.Height;

            // Place the SettingsFlyout inside our Popup window.
            settingsPopup.Child = mypane;

            // Let's define the location of our Popup.
            settingsPopup.SetValue(Canvas.LeftProperty, SettingsPane.Edge == SettingsEdgeLocation.Right ? (Window.Current.Bounds.Width - _settingsWidth) : 0);
            settingsPopup.SetValue(Canvas.TopProperty, 0);
            settingsPopup.IsOpen = true;

            _settingsPopup = settingsPopup;
        }

        void AddSettings()
        {
            SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
        }

        void OnCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            UICommandInvokedHandler handler = new UICommandInvokedHandler(onSettingsCommand);

            SettingsCommand settingsCommand = new SettingsCommand("AccountId", "Account", handler);
            eventArgs.Request.ApplicationCommands.Add(settingsCommand);
        }

        async void Browse_Click(object sender, RoutedEventArgs e)
        {
            SelectedFile = await FilePicker.PickSingleFileAsync();
            if (SelectedFile == null)
            {
                FilePath.Text = "No file selected.";
                UploadButton.IsEnabled = false;
            }
            else
            {
                FilePath.Text = SelectedFile.Name;
                BitmapImage bmpSource = new BitmapImage();
                bmpSource.UriSource = new Uri(SelectedFile.Path);
                UploadButton.IsEnabled = true;
            }
        }

        async void Upload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UploadForm.IsEnabled = false;

                UploadProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                UploadResult.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                ImageUrl.Text = String.Empty;

                Basic<UploadData> result = await _api.Upload(SelectedFile, TitleInput.Text, DescriptionInput.Text, null);

                if (result.Success)
                {
                    UploadStatus.Text = "Upload succeeded.";
                    ImageUrl.Text = result.Data.Link;
                    UploadResult.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    UploadStatus.Text = String.Format("Upload failed. Error code: {0}", result.Status);
                    UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                UploadStatus.Text = "An unknown error has occurred.";
                UploadStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;

                throw ex;
            }
            finally
            {
                UploadProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                UploadForm.IsEnabled = true;
                
            }

            
        }

        private void ImageUrl_GotFocus(object sender, RoutedEventArgs e)
        {
            ImageUrl.SelectAll();
        }

        private async void ShinyButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog msg = new MessageDialog("derp");
            
            await msg.ShowAsync();
        }


    }
}

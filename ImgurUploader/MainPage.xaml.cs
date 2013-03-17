using ImgurUploader.Model;
using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

        private ObservableCollection<QueuedImage> _queuedImages;
        public ObservableCollection<QueuedImage> QueuedImages
        {
            get
            {
                if (_queuedImages == null)
                {
                    _queuedImages = new ObservableCollection<QueuedImage>();
                }

                return _queuedImages;
            }
        }

        private double _settingsWidth = 346;
        private Popup _settingsPopup;


        public MainPage()
        {
            this.InitializeComponent();
            QueuedImagesListView.DataContext = QueuedImages;
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
            UpdateImagePropertyPane();


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

        private async void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<StorageFile> selectedFiles = await FilePicker.PickMultipleFilesAsync();
            foreach (StorageFile selectedFile in selectedFiles)
            {
                if (selectedFile != null)
                {
                    QueuedImage queuedImage = new QueuedImage(selectedFile);
                    QueuedImages.Add(queuedImage);

                    if (QueuedImagesListView.SelectedItems.Count <= 0 && QueuedImagesListView.Items.Count > 0)
                    {
                        QueuedImagesListView.SelectedIndex = 0;
                    }
                }
            }
        }

        private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
        {
            List<QueuedImage> removableQueuedImages = new List<QueuedImage>();
            foreach (object item in QueuedImagesListView.SelectedItems)
            {
                removableQueuedImages.Add(item as QueuedImage);
            }

            foreach (QueuedImage item in removableQueuedImages)
            {
                QueuedImages.Remove(item);
            }
        }

        private async void UploadImagesButton_Click(object sender, RoutedEventArgs e)
        {
            if (QueuedImages.Count <= 0)
            {
                MessageDialog msg = new MessageDialog("You can't upload nothing, silly!");
                await msg.ShowAsync();
            }
            else
            {
                Popup uploadPopup = new Popup();
                //uploadPopup.Closed += OnPopupClosed;
                //Window.Current.Activated += OnWindowActivated;
                uploadPopup.IsLightDismissEnabled = false;
                uploadPopup.Width = Window.Current.Bounds.Width;
                uploadPopup.Height = 180;

                // Add the proper animation for the panel.
                /*
                settingsPopup.ChildTransitions = new TransitionCollection();
                settingsPopup.ChildTransitions.Add(new PaneThemeTransition()
                {
                    Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ?
                           EdgeTransitionLocation.Right :
                           EdgeTransitionLocation.Left
                });
                */

                // Create a SettingsFlyout the same dimenssions as the Popup.
                UploadingProgressPopup mypane = new UploadingProgressPopup(QueuedImages.Count);
                mypane.Width = Window.Current.Bounds.Width;
                mypane.Height = 180;

                // Place the SettingsFlyout inside our Popup window.
                uploadPopup.Child = mypane;

                // Let's define the location of our Popup.
                uploadPopup.SetValue(Canvas.LeftProperty, 0);
                uploadPopup.SetValue(Canvas.TopProperty, (Window.Current.Bounds.Height - 160) / 2);
                uploadPopup.IsOpen = true;





                System.Diagnostics.Debug.WriteLine(String.Format("Now uploading {0} items...", QueuedImages.Count));

                UploadResultCollection uploadedImageResults = new UploadResultCollection();

                try
                {

                    foreach (QueuedImage queuedImage in QueuedImages)
                    {
                        Basic<UploadData> uploadData = await _api.Upload(queuedImage.File, queuedImage.Title, queuedImage.Description, null);
                        UploadImageResult result = new UploadImageResult(queuedImage, uploadData);
                        uploadedImageResults.UploadedImageResults.Add(result); 

                        mypane.CompletedFiles++;
                    }

                    if (QueuedImages.Count >= 1 && uploadedImageResults.SuccessfulUploads.Count > 0)
                    {
                        if (QueuedImages.Count == 1)
                        {
                            this.Frame.Navigate(typeof(UploadResultPage), uploadedImageResults.UploadedImageResults[0]);
                        }
                        else
                        {
                            //Make an album
                            List<string> uploadedImageIds = new List<string>();
                            foreach (UploadImageResult r in uploadedImageResults.SuccessfulUploads)
                            {
                                uploadedImageIds.Add(r.Result.Data.ID);
                            }

                            Basic<AlbumCreateData> createAlbumResult = await _api.CreateAlbum(uploadedImageIds.ToArray(), null, null, null);

                            if (createAlbumResult.Success)
                            {
                                UploadAlbumResult albumResult = new UploadAlbumResult(uploadedImageResults, createAlbumResult);

                                this.Frame.Navigate(typeof(UploadResultPage), albumResult);
                            }
                            else
                            {
                                MessageDialog msg = new MessageDialog("Unable to create album. Please ensure that you are logged in.");
                                await msg.ShowAsync();
                            }
                        }
                    }
                    else
                    {
                        MessageDialog msg = new MessageDialog("Unable to upload anything. Sorry!");
                        await msg.ShowAsync();
                    }
                }
                finally
                {
                    uploadPopup.IsOpen = false;
                }
            }
        }

        private void ItemTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            QueuedImage queuedImage = ImageDetailsPanel.DataContext as QueuedImage;
            if (queuedImage != null)
            {
                queuedImage.Title = ItemTitleTextBox.Text;
            }
        }

        private void ItemDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            QueuedImage queuedImage = ImageDetailsPanel.DataContext as QueuedImage;
            if (queuedImage != null)
            {
                queuedImage.Description = ItemDescriptionTextBox.Text;
            }
        }

        private void QueuedImagesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateImagePropertyPane();
        }

        private void UpdateImagePropertyPane()
        {
            IList<object> selectedImages = QueuedImagesListView.SelectedItems;
            if (selectedImages.Count < 1)
            {
                ImageDetailsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                ImageDetailsPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
    }
}

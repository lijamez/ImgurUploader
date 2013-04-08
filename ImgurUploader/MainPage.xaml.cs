using ImgurUploader.Common;
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
using Windows.UI.ViewManagement;
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
    public sealed partial class MainPage : LayoutAwarePage
    {
        private static bool _settingsAdded = false;

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
        private Popup _accountPopup;


        public MainPage()
        {
            this.InitializeComponent();
            QueuedImagesListView.DataContext = QueuedImages;
            AddSettings();
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
                _accountPopup.IsOpen = false;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UpdateImagePropertyPane();
            UpdateUploadListSwitcher();
            FriendlyAddImageControl.SelectButton.Click += AddImageButton_Click;
        }

        void OnAccountCommand(IUICommand command)
        {
            Popup popup = new Popup();
            popup.Closed += OnPopupClosed;
            Window.Current.Activated += OnWindowActivated;
            popup.IsLightDismissEnabled = true;
            popup.Width = _settingsWidth;
            popup.Height = Window.Current.Bounds.Height;

            // Add the proper animation for the panel.
            popup.ChildTransitions = new TransitionCollection();
            popup.ChildTransitions.Add(new PaneThemeTransition()
            {
                Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ?
                       EdgeTransitionLocation.Right :
                       EdgeTransitionLocation.Left
            });

            // Create a SettingsFlyout the same dimenssions as the Popup.
            AccountFlyout accountFlyout = new AccountFlyout(this.Frame);
            accountFlyout.Width = _settingsWidth;
            accountFlyout.Height = Window.Current.Bounds.Height;
            accountFlyout.DataContext = this;

            // Place the SettingsFlyout inside our Popup window.
            popup.Child = accountFlyout;

            // Let's define the location of our Popup.
            popup.SetValue(Canvas.LeftProperty, SettingsPane.Edge == SettingsEdgeLocation.Right ? (Window.Current.Bounds.Width - _settingsWidth) : 0);
            popup.SetValue(Canvas.TopProperty, 0);
            popup.IsOpen = true;

            _accountPopup = popup;
        }

        void OnPrivacyPolicyCommand(IUICommand command)
        {
            this.Frame.Navigate(typeof(PrivacyPolicy));
        }

        void AddSettings()
        {
            if (!_settingsAdded)
            {
                SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
                _settingsAdded = true;
            }
        }

        void OnCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            UICommandInvokedHandler handler;

            handler = new UICommandInvokedHandler(OnAccountCommand);
            SettingsCommand settingsCommand = new SettingsCommand("AccountId", "Account", handler);
            eventArgs.Request.ApplicationCommands.Add(settingsCommand);

            handler = new UICommandInvokedHandler(OnPrivacyPolicyCommand);
            SettingsCommand privacyPolicyCommand = new SettingsCommand("PrivacyPolicy", "Privacy Policy", handler);
            eventArgs.Request.ApplicationCommands.Add(privacyPolicyCommand);
        }

        private async void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            // We can't open a FilePicker when the app is snapped. 
            // Try to unsnap the app
            bool canOpenFilePicker = true;
            if (Windows.UI.ViewManagement.ApplicationView.Value == ApplicationViewState.Snapped)
            {
                canOpenFilePicker = Windows.UI.ViewManagement.ApplicationView.TryUnsnap();
            }

            if (canOpenFilePicker)
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

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (QueuedImagesListView.SelectedItems.Count >= QueuedImagesListView.Items.Count)
            {
                QueuedImagesListView.SelectedItems.Clear();
            }
            else
            {
                QueuedImagesListView.SelectAll();
            }
        }

        private bool _uploadCancelRequested = false;
        private async void UploadImagesButton_Click(object sender, RoutedEventArgs e)
        {
            if (QueuedImages.Count <= 0)
            {
                MessageDialog msg = new MessageDialog("You can't upload nothing, silly!");
                await msg.ShowAsync();
            }
            else
            {
                Rect windowBounds = Window.Current.Bounds;

                Popup uploadPopup = new Popup();
                //uploadPopup.Closed += OnPopupClosed;
                //Window.Current.Activated += OnWindowActivated;
                uploadPopup.IsLightDismissEnabled = false;
                uploadPopup.Width = windowBounds.Width;
                uploadPopup.Height = windowBounds.Height;

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
                mypane.UploadCancelButton.Click += UploadCancel;
                mypane.Width = windowBounds.Width;
                mypane.Height = windowBounds.Height;

                // Place the SettingsFlyout inside our Popup window.
                uploadPopup.Child = mypane;

                // Let's define the location of our Popup.
                uploadPopup.SetValue(Canvas.LeftProperty, 0);
                uploadPopup.SetValue(Canvas.TopProperty, 0);
                uploadPopup.IsOpen = true;


                System.Diagnostics.Debug.WriteLine(String.Format("Now uploading {0} items...", QueuedImages.Count));

                UploadResultCollection uploadedImageResults = new UploadResultCollection();

                try
                {

                    foreach (QueuedImage queuedImage in QueuedImages)
                    {
                        if (_uploadCancelRequested) { throw new OperationCanceledException("Cancelled"); }

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

                            if (_uploadCancelRequested) { throw new OperationCanceledException("Cancelled"); }
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
                catch (OperationCanceledException) { }
                finally
                {
                    _uploadCancelRequested = false;
                    uploadPopup.IsOpen = false;
                }
            }
        }

        private void UploadCancel(object sender, RoutedEventArgs e)
        {
            _uploadCancelRequested = true;
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
            UpdateAppBarIcons();
            UpdateUploadListSwitcher();
            
        }

        //TODO: Also need to call this when the user rotates the device
        private void UpdateImagePropertyPane()
        {
            IList<object> selectedImages = QueuedImagesListView.SelectedItems;
            if (selectedImages.Count < 1)
            {
                ImageDetailsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                ImageDetailsPanel.Width = Window.Current.Bounds.Width / 2;
                System.Diagnostics.Debug.WriteLine(ImageDetailsPanel.Width);
                ImageDetailsPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void UpdateAppBarIcons()
        {
            bool moreThanOneImageSelected = QueuedImagesListView.SelectedItems.Count > 0;

            RemoveImageButton.IsEnabled = moreThanOneImageSelected;
            UploadBottomAppBar.IsOpen = moreThanOneImageSelected;
        }

        private void UpdateUploadListSwitcher()
        {
            if (QueuedImagesListView.Items.Count <= 0)
            {
                QueuedImagesListView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                FriendlyAddImageControl.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                QueuedImagesListView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                FriendlyAddImageControl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }
    }
}

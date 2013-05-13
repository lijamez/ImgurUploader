using ImgurUploader.Common;
using ImgurUploader.Model;
using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ApplicationSettings;
using Windows.UI.Notifications;
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

        private ObservableCollection<QueuedFile> _queuedImages;
        public ObservableCollection<QueuedFile> QueuedFiles
        {
            get
            {
                if (_queuedImages == null)
                {
                    _queuedImages = new ObservableCollection<QueuedFile>();
                }

                return _queuedImages;
            }
        }
        private AlbumPreferences _albumPreferences;
        public AlbumPreferences AlbumPreferences
        {
            get
            {
                if (_albumPreferences == null)
                {
                    _albumPreferences = new AlbumPreferences();
                }

                return _albumPreferences;
            }
        }

        private double _settingsWidth = 346;
        private Popup _accountPopup;
        private Popup _aboutPopup;


        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            QueuedImagesListView.DataContext = QueuedFiles;
            MainAd.DataContext = InAppPurchases.Instance;
            SnappedAd.DataContext = InAppPurchases.Instance;
            ImageDetailsPanelWrapper.DataContext = Window.Current.Bounds;

            AddSettings();
            UpdateAppBarIcons();

            AlbumPreferencesStackPanel.DataContext = QueuedFiles;
            AlbumPreferencesStackPanelInner.DataContext = AlbumPreferences;
        }

        private void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            e.Request.FailWithDisplayText("Please upload something first before trying to share.");
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
                if (_accountPopup != null) _accountPopup.IsOpen = false;
                if (_aboutPopup != null) _aboutPopup.IsOpen = false;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UpdateImagePropertyPane();
            UpdateUploadListSwitcher();
            FriendlyAddImageControl.SelectButton.Click += AddImageButton_Click;

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ShareTextHandler;

#if DEBUG
            await LoadInAppPurchaseProxyFileAsync();
#endif

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested -= ShareTextHandler;

#if DEBUG
            if (licenseChangeHandler != null)
            {
                CurrentAppSimulator.LicenseInformation.LicenseChanged -= licenseChangeHandler;
            }
#endif
        }

#if DEBUG
        LicenseChangedEventHandler licenseChangeHandler = null;

        private async Task LoadInAppPurchaseProxyFileAsync()
        {
            StorageFolder proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("data");
            StorageFile proxyFile = await proxyDataFolder.GetFileAsync("in-app-purchase.xml");
            licenseChangeHandler = new LicenseChangedEventHandler(InAppPurchaseRefreshScenario);
            CurrentAppSimulator.LicenseInformation.LicenseChanged += licenseChangeHandler;
            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
        }

        private void InAppPurchaseRefreshScenario() { }

#endif

        Popup CreateFlyoutPopup(System.Type popupType)
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
            FlyoutPage flyoutPage = (FlyoutPage)Activator.CreateInstance(popupType);
            flyoutPage.Width = _settingsWidth;
            flyoutPage.Height = Window.Current.Bounds.Height;
            flyoutPage.DataContext = this;

            // Place the SettingsFlyout inside our Popup window.
            popup.Child = flyoutPage;

            // Let's define the location of our Popup.
            popup.SetValue(Canvas.LeftProperty, SettingsPane.Edge == SettingsEdgeLocation.Right ? (Window.Current.Bounds.Width - _settingsWidth) : 0);
            popup.SetValue(Canvas.TopProperty, 0);
            popup.IsOpen = true;

            return popup;
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

            handler = new UICommandInvokedHandler(delegate(IUICommand c)
            {
                _accountPopup = CreateFlyoutPopup(typeof(AccountFlyout));
            });
            SettingsCommand settingsCommand = new SettingsCommand("AccountId", "Account", handler);
            eventArgs.Request.ApplicationCommands.Add(settingsCommand);

            handler = new UICommandInvokedHandler(delegate(IUICommand c)
            {
                this.Frame.Navigate(typeof(PrivacyPolicy));
            });
            SettingsCommand privacyPolicyCommand = new SettingsCommand("PrivacyPolicy", "Privacy Policy", handler);
            eventArgs.Request.ApplicationCommands.Add(privacyPolicyCommand);

            handler = new UICommandInvokedHandler(delegate(IUICommand c)
            {
                _aboutPopup = CreateFlyoutPopup(typeof(AboutFlyout));
            });
            SettingsCommand aboutCommand = new SettingsCommand("About", "About", handler);
            eventArgs.Request.ApplicationCommands.Add(aboutCommand);
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
                IReadOnlyList<StorageFile> selectedFiles = null;
                try
                {
                    selectedFiles = await FilePicker.PickMultipleFilesAsync();
                }
                catch (UnauthorizedAccessException)
                {
                    System.Diagnostics.Debug.WriteLine("An UnauthorizedAccessException was thrown by the FilePicker which is really weird...");
                }

                if (selectedFiles != null)
                {
                    foreach (StorageFile selectedFile in selectedFiles)
                    {
                        if (selectedFile != null)
                        {
                            QueuedFile queuedImage = new QueuedFile(selectedFile);
                            QueuedFiles.Add(queuedImage);

                            if (QueuedImagesListView.SelectedItems.Count <= 0 && QueuedImagesListView.Items.Count > 0)
                            {
                                QueuedImagesListView.SelectedIndex = 0;
                            }
                        }
                    }
                }
            }
        }


        private void MoveImageUpButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = QueuedImagesListView.SelectedIndex;
            if (selectedIndex > 0)
            {
                QueuedFile movedItem = QueuedFiles[selectedIndex];
                QueuedFile swappedItem = QueuedFiles[selectedIndex - 1];

                QueuedFiles[selectedIndex - 1] = movedItem;
                QueuedFiles[selectedIndex] = swappedItem;

                QueuedImagesListView.SelectedIndex--;
            }
        }

        private void MoveImageDownButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = QueuedImagesListView.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < QueuedFiles.Count - 1)
            {
                QueuedFile movedItem = QueuedFiles[selectedIndex];
                QueuedFile swappedItem = QueuedFiles[selectedIndex + 1];

                QueuedFiles[selectedIndex + 1] = movedItem;
                QueuedFiles[selectedIndex] = swappedItem;

                QueuedImagesListView.SelectedIndex++;
            }
        }


        private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
        {
            List<QueuedFile> removableQueuedImages = new List<QueuedFile>();
            foreach (object item in QueuedImagesListView.SelectedItems)
            {
                removableQueuedImages.Add(item as QueuedFile);
            }

            foreach (QueuedFile item in removableQueuedImages)
            {
                QueuedFiles.Remove(item);
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

        private CancellationTokenSource _uploadCancellationTokenSource;
        private async void UploadImagesButton_Click(object sender, RoutedEventArgs e)
        {
            if (QueuedFiles.Count <= 0)
            {
                MessageDialog msg = new MessageDialog("You can't upload nothing, silly!");
                await msg.ShowAsync();
            }
            else
            {
                Rect windowBounds = Window.Current.Bounds;

                Popup uploadPopup = new Popup();
                uploadPopup.IsLightDismissEnabled = false;
                uploadPopup.Width = windowBounds.Width;
                uploadPopup.Height = windowBounds.Height;

                // Add the proper animation for the panel.
                /*
                uploadPopup.ChildTransitions = new TransitionCollection();
                uploadPopup.ChildTransitions.Add(new PaneThemeTransition()
                {
                    Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ?
                           EdgeTransitionLocation.Right :
                           EdgeTransitionLocation.Left
                });
                */

                UploadingProgressPopup mypane = new UploadingProgressPopup(QueuedFiles.Count);
                mypane.UploadCancelButton.Click += UploadCancel;
                mypane.Width = windowBounds.Width;
                mypane.Height = windowBounds.Height;

                uploadPopup.Child = mypane;

                // Let's define the location of our Popup.
                uploadPopup.SetValue(Canvas.LeftProperty, 0);
                uploadPopup.SetValue(Canvas.TopProperty, 0);
                uploadPopup.IsOpen = true;


                System.Diagnostics.Debug.WriteLine(String.Format("Now uploading {0} items...", QueuedFiles.Count));

                UploadResultCollection uploadedImageResults = new UploadResultCollection();
                Exception exception = null;

                try
                {
                    DateTime startTime = DateTime.UtcNow;

                    _uploadCancellationTokenSource = new CancellationTokenSource();

                    string message = "Upload Completed";

                    foreach (QueuedFile queuedImage in QueuedFiles)
                    {
                        if (_uploadCancellationTokenSource.IsCancellationRequested) { throw new TaskCanceledException("Cancelled"); }

                        Basic<UploadData> uploadData = await _api.Upload(queuedImage.File, queuedImage.Title, queuedImage.Description, null, _uploadCancellationTokenSource.Token);
                        UploadImageResult result = new UploadImageResult(queuedImage, uploadData);
                        uploadedImageResults.UploadedImageResults.Add(result);

                        mypane.CompletedFiles++;
                    }

                    if (QueuedFiles.Count >= 1 && uploadedImageResults.SuccessfulUploads.Count > 0)
                    {
                        FinishedUploadResult finishedResult = null;

                        if (QueuedFiles.Count == 1)
                        {
                            finishedResult = new FinishedUploadResult(uploadedImageResults, null);
                            finishedResult.StartDate = startTime;
                            finishedResult.FinishDate = DateTime.UtcNow;
                        }
                        else
                        {
                            //Make an album
                            List<string> uploadedImageIds = new List<string>();
                            foreach (UploadImageResult r in uploadedImageResults.SuccessfulUploads)
                            {
                                uploadedImageIds.Add(r.Result.Data.ID);
                            }

                            if (_uploadCancellationTokenSource.IsCancellationRequested) { throw new TaskCanceledException("Cancelled"); }
                            Basic<AlbumCreateData> createAlbumResult = await _api.CreateAlbum(uploadedImageIds.ToArray(), AlbumPreferences.Title, AlbumPreferences.Description, AlbumPreferences.Cover, _uploadCancellationTokenSource.Token);

                            if (createAlbumResult.Success)
                            {
                                if (!String.Equals(AlbumPreferences.Privacy, AlbumPreferences.DEFAULT_PRIVACY) || !String.Equals(AlbumPreferences.Layout, AlbumPreferences.DEFAULT_LAYOUT))
                                {
                                    Basic<Boolean> albumUpdateResult = await _api.UpdateAlbum(createAlbumResult.Data.DeleteHash, null, null, null, AlbumPreferences.Privacy, AlbumPreferences.Layout, null, _uploadCancellationTokenSource.Token);
                                }

                                finishedResult = new FinishedUploadResult(uploadedImageResults, createAlbumResult);
                                finishedResult.StartDate = startTime;
                                finishedResult.FinishDate = DateTime.UtcNow;
                            }
                            else
                            {
                                message = "Unable to create album.";
                                MessageDialog msg = new MessageDialog(message);
                                await msg.ShowAsync();
                            }
                        }

                        if (finishedResult != null)
                        {
                            App.UploadHistoryMgr.UploadHistory.Insert(0, finishedResult);

                            QueuedFiles.Clear();

                            this.Frame.Navigate(typeof(UploadResultPage), finishedResult);
                        }


                    }
                    else
                    {
                        if (uploadedImageResults.FailedUploads.Count == 1)
                        {
                            message = uploadedImageResults.FailedUploads[0].Result.Data.Error;
                        }
                        else
                        {
                            message = "Unable to upload anything. Sorry!";
                        }
                        MessageDialog msg = new MessageDialog(message);
                        await msg.ShowAsync();
                    }


                    //TODO: Only show toast when the app is in the background
                    ToastNotification toast = Toaster.MakeToast(message, "", null);
                    ToastNotificationManager.CreateToastNotifier().Show(toast);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    uploadPopup.IsOpen = false;
                }

                if (exception != null)
                {
                    MessageDialog msg = new MessageDialog(exception.Message, "Something went wrong. Please try again.");
                    await msg.ShowAsync();
                }
            }
        }

        private void UploadCancel(object sender, RoutedEventArgs e)
        {
            if (_uploadCancellationTokenSource != null)
            {
                _uploadCancellationTokenSource.Cancel();

                Button cancelButton = sender as Button;
                if (cancelButton != null)
                {
                    cancelButton.IsEnabled = false;
                    cancelButton.Content = "Cancelling...";
                }
            }
        }

        private void ItemTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            QueuedFile queuedImage = textBox.DataContext as QueuedFile;
            if (queuedImage != null)
            {
                queuedImage.Title = ItemTitleTextBox.Text;
            }
        }

        private void ItemDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            QueuedFile queuedImage = textBox.DataContext as QueuedFile;
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

        //TODO: Also need to call this when the user rotates the device, or moves the window to another screen, etc.
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

        private void UpdateAppBarIcons()
        {
            bool someImagesSelected = QueuedImagesListView.SelectedItems.Count > 0;
            bool exactlyOneImageSelected = QueuedImagesListView.SelectedItems.Count == 1;

            SelectAllButton.IsEnabled = _queuedImages.Count > 0;

            RemoveImageButton.IsEnabled = someImagesSelected;
            UploadBottomAppBar.IsOpen = someImagesSelected;
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

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UploadHistory.UploadHistoryPage));
        }

        private void AlbumLayoutBlogRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            AlbumPreferences.Layout = ImgurAPI.ALBUM_LAYOUT_BLOG;
        }

        private void AlbumLayoutBlogGridButton_Checked(object sender, RoutedEventArgs e)
        {
            AlbumPreferences.Layout = ImgurAPI.ALBUM_LAYOUT_GRID;
        }

        private void AlbumLayoutBlogHorizontalButton_Checked(object sender, RoutedEventArgs e)
        {
            AlbumPreferences.Layout = ImgurAPI.ALBUM_LAYOUT_HORIZONTAL;
        }

        private void AlbumLayoutBlogVerticalButton_Checked(object sender, RoutedEventArgs e)
        {
            AlbumPreferences.Layout = ImgurAPI.ALBUM_LAYOUT_VERTICAL;
        }
    }
}

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
    public sealed partial class UploadingProgressPopup : Page, UploadProgressListener
    {
        private int _totalFiles;
        public int TotalFiles
        {
            get
            {
                return _totalFiles;
            }
            set
            {
                _totalFiles = value;
            }
        }

        private int _completedFiles = 0;
        public int CompletedFiles
        {
            get
            {
                return _completedFiles;
            }
            set
            {
                _completedFiles = value;
            }
        }

        public UploadingProgressPopup(int totalFiles)
        {
            this.InitializeComponent();
            TotalFiles = totalFiles;
            updateProgressBar();

        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void updateProgressBar()
        {
            if (TotalFiles > 0)
            {
                if (CompletedFiles == 0)
                {
                    UploadProgressBar.IsIndeterminate = true;
                }
                else
                {
                    UploadProgressBar.IsIndeterminate = false;
                    UploadProgressBar.Value = (double)CompletedFiles / TotalFiles;
                }

                if (CompletedFiles < TotalFiles)
                {
                    UploadProgressTextBlock.Text = String.Format("Uploading file {0} of {1}", CompletedFiles + 1, TotalFiles);
                }
                else
                {
                    UploadProgressTextBlock.Text = "Almost there...";
                }
            }
            else
            {
                UploadProgressBar.IsIndeterminate = true;
                UploadProgressTextBlock.Text = String.Empty;
            }
            
        }

        public Button UploadCancelButton
        {
            get { return CancelButton; }
        }

        public void NotifyProgression(int count)
        {
            if (count != 0)
            {
                CompletedFiles += count;
                updateProgressBar();
            }
        }

        public void SetMaxProgression(int max)
        {
            if (max > 0 && max != TotalFiles)
            {
                TotalFiles = max;
                updateProgressBar();
            }
        }

        public void NotifyProgressionMessage(string message)
        {
            //Do nothing
        }
    }
}

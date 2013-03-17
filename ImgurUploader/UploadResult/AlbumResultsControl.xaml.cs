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
    public sealed partial class AlbumResultsControl : Page
    {
        private UploadAlbumResult _results;
        public UploadAlbumResult Results
        {
            get { return _results; }
        }


        public AlbumResultsControl(UploadAlbumResult r)
        {
            this.InitializeComponent();
            _results = r;

            CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("Link to Album (email & IM)", String.Format("http://imgur.com/a/{0}", r.Results.Data.ID), true));
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }
    }
}

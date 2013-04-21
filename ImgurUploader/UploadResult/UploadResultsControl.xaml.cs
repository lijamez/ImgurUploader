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
    public sealed partial class UploadResultsControl : UserControl
    {
        private FinishedUploadResult _result;
        public FinishedUploadResult Result
        {
            get { return _result; }
        }

        public UploadResultsControl(FinishedUploadResult r)
        {
            this.InitializeComponent();
            _result = r;

            if (_result.AlbumCreateResults == null)
            {
                //Assume single image upload
                if (_result.Images.SuccessfulUploads != null && _result.Images.SuccessfulUploads.Count >= 1)
                {
                    UploadImageResult imageResult = _result.Images.SuccessfulUploads[0];

                    CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("Link (email & IM)", String.Format("http://imgur.com/{0}", imageResult.Result.Data.ID), true));
                    CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("Direct Link (email & IM)", String.Format("{0}", imageResult.Result.Data.Link), true));
                    CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("HTML Image (websites & blogs)", String.Format(@"<a href=""http://imgur.com/{0}""><img src=""{1}"" alt="" title=""Hosted by imgur.com"" /></a>", imageResult.Result.Data.ID, imageResult.Result.Data.Link), false));
                    CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("BBCode (message boards & forums)", String.Format(@"[IMG]{0}[/IMG]", imageResult.Result.Data.Link), false));
                    CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("Linked BBCode (message boards)", String.Format(@"[URL=http://imgur.com/{0}][IMG]{1}[/IMG][/URL]", imageResult.Result.Data.ID, imageResult.Result.Data.Link), false));
                    CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("Markdown Link (reddit comment)", String.Format(@"[Imgur]({0})", imageResult.Result.Data.Link), false));
                }
            }
            else
            {
                //Assume album upload
                CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("Link to Album (email & IM)", String.Format("http://imgur.com/a/{0}", r.AlbumCreateResults.Data.ID), true));
            }
        }


    }
}

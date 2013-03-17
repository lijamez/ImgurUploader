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
    public sealed partial class ImageResultsControl : UserControl
    {
        private UploadImageResult _result;
        public UploadImageResult Result
        {
            get { return _result; }
        }

        public ImageResultsControl(UploadImageResult r)
        {
            this.InitializeComponent();
            _result = r;

            CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("Link (email & IM)", String.Format("http://imgur.com/{0}", _result.Result.Data.ID), true));
            CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("Direct Link (email & IM)", String.Format("http://i.imgur.com/{0}{1}", _result.Result.Data.ID, _result.Image.File.FileType), true));
            CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("HTML Image (websites & blogs)", String.Format(@"<a href=""http://imgur.com/{0}""><img src=""http://i.imgur.com/{0}{1}"" alt="" title=""Hosted by imgur.com"" /></a>", _result.Result.Data.ID, _result.Image.File.FileType), false));
            CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("BBCode (message boards & forums)", String.Format(@"[IMG]http://i.imgur.com/{0}{1}[/IMG]", _result.Result.Data.ID, _result.Image.File.FileType), false));
            CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("Linked BBCode (message boards)", String.Format(@"[URL=http://imgur.com/{0}][IMG]http://i.imgur.com/{0}{1}[/IMG][/URL]", _result.Result.Data.ID, _result.Image.File.FileType), false));
            CopyableLinksStackPanel.Children.Add(new CopyableLinkControl("Markdown Link (reddit comment)", String.Format(@"[Imgur](http://i.imgur.com/{0}{1})", _result.Result.Data.ID, _result.Image.File.FileType), false));
        }


    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ImgurUploader
{
    public class QueuedImage : ObservableObject
    {
        private StorageFile _file;
        public StorageFile File
        {
            get
            {
                return _file;
            }
            set
            {
                _file = value;
                NotifyPropertyChanged();
            }
        }

        public string FileName
        {
            get
            {
                if (File != null)
                    return File.Name;
                else
                    return "null";
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                NotifyPropertyChanged();
            }
        }

        public QueuedImage(StorageFile file)
        {
            File = file;
            GenerateThumbnail();
        }

        private BitmapImage _thumbnail;
        public BitmapImage Thumbnail
        {
            get
            {
                return _thumbnail;
            }
            set
            {
                _thumbnail = value;
                NotifyPropertyChanged();
            }
        }

        private async void GenerateThumbnail()
        {
            StorageItemThumbnail thumbnail = await File.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.ListView);
            if (thumbnail != null)
            {
                BitmapImage img = new BitmapImage();
                img.SetSource(thumbnail);
                Thumbnail = img;
            }
        }
    }
}

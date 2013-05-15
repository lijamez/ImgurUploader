using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ImgurUploader
{
    public class QueuedFile : QueuedItem
    {
        public QueuedFile() { }

        public QueuedFile(StorageFile file)
        {
            File = file;
        }


        private StorageFile _file;

        [XmlIgnore()]
        //Do NOT assume that this variable is not null
        public StorageFile File
        {
            get
            {
                return _file;
            }
            set
            {
                _file = value;
                GenerateThumbnail();
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
        
        private BitmapImage _thumbnail;
        [XmlIgnore()]
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
            try
            {
                if (File != null)
                {
                    StorageItemThumbnail thumbnail = await File.GetThumbnailAsync(ThumbnailMode.ListView, 200);
                    if (thumbnail != null)
                    {
                        BitmapImage img = new BitmapImage();
                        img.SetSource(thumbnail);
                        Thumbnail = img;
                    }
                }
            }
            catch (Exception) { }
        }
    }
}

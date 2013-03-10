using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

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
        }


    }
}

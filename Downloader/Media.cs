using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace YouTubeDownloader.Downloader
{
    public class Media : INotifyPropertyChanged
    {
        private string link;
        private SolidColorBrush background;
        private string name;
        private string thumbnail;
        private bool check;
        private int typeIndex;
        private int qualityIndex;
        private bool downloadAvailable;
        private List<string> types;
        private List<string> qualityes;
        public event PropertyChangedEventHandler? PropertyChanged;

        public SolidColorBrush Background
        {
            get { return background; }
            set { background = value; OnPropertyChanged(); }
        }
        public string Link
        {
            get { return link; }
            set { link = value; OnPropertyChanged(); }
        }
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(); }
        }
        public string Thumbnail
        {
            get { return thumbnail; }
            set { thumbnail = value; OnPropertyChanged(); }
        }
        public bool Check
        {
            get { return check; }
            set { check = value; OnPropertyChanged(); }
        }
        public int TypeIndex
        {
            get { return typeIndex; }
            set { typeIndex = value; Qualityes = SetQualities(Types[TypeIndex]); QualityIndex = 0; OnPropertyChanged(); }
        }
        public int QualityIndex
        {
            get { return qualityIndex; }
            set { qualityIndex = value; OnPropertyChanged(); }
        }
        public List<string> Types
        {
            get { return types; }
            set { types = value; OnPropertyChanged(); }
        }
        public List<string> Qualityes
        {
            get { return qualityes; }
            set { qualityes = value; OnPropertyChanged(); }
        }
        public bool DownloadAvailable
        {
            get { return downloadAvailable; }
            set { downloadAvailable = value; OnPropertyChanged(); }
        }
        internal Media(string link, string name, string thumbnail)
        {
            Background = System.Windows.Media.Brushes.White;
            Link = link;
            Name = name;
            Thumbnail = thumbnail;
            Check = false;
            DownloadAvailable = true;
            Types = new List<string>
            {
                "Video",
                "Audio"
            };
            TypeIndex = 0;
        }
        public string Download(string path, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var youTube = new YouTube();

            if (Types[TypeIndex] == "Video")
            {
                return youTube.DownloadVideo(Link, Name, path, Qualityes[QualityIndex]);
            }
            else if (Types[TypeIndex] == "Audio")
            {
                return youTube.DownloadAudio(Link, Name, path, Qualityes[QualityIndex]);
            }
            else
            {
                return youTube.DownloadInvalidType(Link, Name);
            }
        }
        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private List<string> SetQualities(string type)
        {
            if (type == "Video")
            {
                return new List<string>
                {
                    "Highest",
                    "1080p",
                    "720p",
                    "480p"
                };
            }
            else
            {
                return new List<string>
                {
                    "Default",
                    "320k",
                    "256k",
                    "128k",
                    "64k"
                };
            }
        }
    }
}

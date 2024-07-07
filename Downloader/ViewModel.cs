using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace YouTubeDownloader.Downloader
{
    public class ViewModel : INotifyPropertyChanged
    {
        // private variables for binding
        private List<Media> mediaFiles;
        private Visibility filesVisibility;
        private Visibility linkBackround;
        private Visibility nameBeforeAfterTextBackround;
        private Visibility pathBackround;
        private SolidColorBrush informationForeground;
        private List<string> types;
        private List<string> qualityes;
        private string output;
        private string information;
        private string link;
        private string nameBeforeAfterText;
        private string percentage;
        private string folder;
        private int selectedType;
        private int selectedQuality;
        private bool check;
        private bool downloadAvailable;
        private CancellationTokenSource cancelToken;

        // private variables for internal logic...........................................................................
        private int selected;
        private int downloaded;
        private bool checkingall;

        // binding variables..............................................................................................
        public List<Media> MediaFiles
        {
            get { return mediaFiles; }
            private set { mediaFiles = value; OnPropertyChanged(); }
        }
        public Visibility FilesVisibility
        {
            get { return filesVisibility; }
            private set { filesVisibility = value; NameBeforeAfterTextBackround = value; OnPropertyChanged(); }
        }
        public Visibility LinkBackround
        {
            get { return linkBackround; }
            private set { linkBackround = value; OnPropertyChanged(); }
        }
        public Visibility NameBeforeAfterTextBackround
        {
            get { return nameBeforeAfterTextBackround; }
            private set { nameBeforeAfterTextBackround = value; OnPropertyChanged(); }
        }
        public Visibility PathBackround
        {
            get { return pathBackround; }
            private set { pathBackround = value; OnPropertyChanged(); }
        }
        public SolidColorBrush InformationForeground
        {
            get { return informationForeground; }
            private set { informationForeground = value; OnPropertyChanged(); }
        }
        public List<string> Types
        {
            get { return types; }
            private set { types = value; OnPropertyChanged(); }
        }
        public List<string> Qualityes
        {
            get { return qualityes; }
            set { qualityes = value; OnPropertyChanged(); }
        }
        public string Output
        {
            get { return output; }
            private set { output = value; OnPropertyChanged(); }
        }
        public string Information
        {
            get { return information; }
            private set { information = value; OnPropertyChanged(); }
        }
        public string Link
        {
            get { return link; }
            set { link = value; LinkChanged(); OnPropertyChanged(); }
        }
        public string NameBeforeAfterText
        {
            get { return nameBeforeAfterText; }
            set { nameBeforeAfterText = value; NameBeforeAfterTextChanged(); OnPropertyChanged(); }
        }
        public string Percentage
        {
            get { return percentage; }
            private set { percentage = value; OnPropertyChanged(); }
        }
        public string Folder
        {
            get { return folder; }
            set { folder = value; FolderChanged(); OnPropertyChanged(); }
        }
        public int SelectedType
        {
            get { return selectedType; }
            set { selectedType = value; Qualityes = SetQualities(Types[SelectedType]); ChangeTypes(); SelectedQuality = 0; OnPropertyChanged(); }
        }
        public int SelectedQuality
        {
            get { return selectedQuality; }
            set { selectedQuality = value; ChangeQualities(); OnPropertyChanged(); }
        }
        public bool Check
        {
            get { return check; }
            set { check = value; CheckAll(); OnPropertyChanged(); }
        }
        public bool DownloadAvailable
        {
            get { return downloadAvailable; }
            set { downloadAvailable = value; FreezeUnfreezeDownload(); OnPropertyChanged(); }
        }

        // constructor....................................................................................................
        public ViewModel()
        {
            Types = new List<string> { "Video", "Audio" };
            checkingall = true;
            Initialize();
            ClearInformation();
            ClearLink();
            ClearOutput();
        }

        // public methods.................................................................................................
        public void Refresh()
        {
            Initialize();
            GetMediaFiles();
        }
        public void PasteLink()
        {
            Link = System.Windows.Clipboard.GetText();
        }
        public void ClearLink()
        {
            Link = "";
        }
        public void PasteNameBeforeAfterText()
        {
            NameBeforeAfterText = System.Windows.Clipboard.GetText();
        }
        public void ClearNameBeforeAfterText()
        {
            NameBeforeAfterText = "";
        }
        public void Before()
        {
            foreach (var m in MediaFiles)
            {
                m.Name = NameBeforeAfterText + m.Name;
            }
        }
        public void After()
        {
            foreach (var m in MediaFiles)
            {
                m.Name = m.Name + NameBeforeAfterText;
            }
        }
        public async void DownloadAll()
        {
            DownloadAvailable = false;
            cancelToken = new CancellationTokenSource();
            ClearInformation();
            if (selected == 0)
            {
                DownloadNothing();
            }
            else
            { 
                downloaded = 0;
                InitializeFilesBackground();
                ClearOutput();
                ShowStatus();
                CalculatePercentage();
                if (ValidPath())
                {
                    Output += "Downloading..." + Environment.NewLine;
                    foreach (var m in MediaFiles)
                    {
                        if (m.Check && !cancelToken.Token.IsCancellationRequested)
                        {
                            var result = await Task.Run(() => m.Download(Folder, cancelToken.Token));
                            ProcessResult(result, m);
                            downloaded++;
                            ShowStatus();
                            CalculatePercentage();
                        }
                    }
                    Output += "Download Completed!" + Environment.NewLine;
                }
            }
            DownloadWasCanceled();
            cancelToken.Dispose();
            cancelToken = null;
            DownloadAvailable = true;
        }
        public async void Download(string link)
        {
            DownloadAvailable = false;
            cancelToken = new CancellationTokenSource();
            if (ValidPath())
            {
                foreach (var m in MediaFiles)
                {
                    if (m.Link == link)
                    {
                        m.Background = System.Windows.Media.Brushes.White;
                        var result = await Task.Run(() => m.Download(Folder, cancelToken.Token));
                        ProcessResult(result, m);
                        break;
                    }
                }
            }
            else
            {
                PathError();
            }
            DownloadWasCanceled();
            cancelToken.Dispose();
            cancelToken = null;
            DownloadAvailable = true;
        }
        public void CheckFile(string link)
        {
            foreach (var m in MediaFiles)
            {
                if (m.Link == link)
                {
                    CheckUncheckFile(m);
                }
            }
        }
        public async void Cancel()
        {
            if (cancelToken != null)
            {
                cancelToken.Cancel();
                Output += "Download Canceled!" + Environment.NewLine;
            }
        }

        // private methods.................................................................................................
        private void Initialize()
        {
            MediaFiles = new List<Media>();
            downloaded = 0;
            selected = 0;
            Check = false;
            DownloadAvailable = true;
        }

        private void ProcessResult(string result, Media file) 
        {
            if (result.StartsWith("[ERROR]"))
            {
                output += result;
                file.Background = System.Windows.Media.Brushes.Red;
            }
            else { file.Background = System.Windows.Media.Brushes.Green; }
        }
        private void InitializeFilesBackground()
        {
            foreach(var m in MediaFiles)
            {
                m.Background = System.Windows.Media.Brushes.White;
            }
        }
        private void DownloadWasCanceled()
        {
            if (cancelToken.IsCancellationRequested)
            {;
                Output += "Download Canceled!" + Environment.NewLine;
            }
        }
        private void FreezeUnfreezeDownload()
        {
            foreach (var m in MediaFiles)
            {
                m.DownloadAvailable = DownloadAvailable;
            }
        }
        private void CheckUncheckFile(Media file)
        {
            checkingall = false;
            if (file.Check)
            {
                selected++;
                IfCheckAll();
            }
            else
            {
                selected--;
                Check = false;
            }
            checkingall = true;
        }
        private void IfCheckAll()
        {
            if (selected == MediaFiles.Count)
            {
                Check = true;
            }
        }
        private void CheckAll()
        {
            if (checkingall)
            {
                foreach (var m in MediaFiles)
                {
                    m.Check = Check;
                }
                if (Check)
                {
                    selected = MediaFiles.Count;
                }
                else
                {
                    selected = 0;
                }
            }
        }
        private void LinkChanged()
        {
            ClearInformation();
            FilesVisibility = Visibility.Hidden;
            LinkBackround = Visibility.Visible;
            if (Link.Length > 0)
            {
                LinkBackround = Visibility.Hidden;
                ValidLink();
            }
        }
        private void NameBeforeAfterTextChanged()
        {
            NameBeforeAfterTextBackround = Visibility.Visible;
            if (NameBeforeAfterText.Length > 0)
            {
                NameBeforeAfterTextBackround = Visibility.Hidden;
            }
        }
        private void FolderChanged()
        {
            PathBackround = Visibility.Visible;
            if (Folder.Length > 0)
            {
                PathBackround = Visibility.Hidden;
            }
        }
        private void ValidLink()
        {
            if (Link.StartsWith("https://www.youtube.com/watch") || Link.StartsWith("https://www.youtube.com/playlist"))
            {
                FilesVisibility = Visibility.Visible;
                GetMediaFiles();
            }
            else
            {
                LinkError();
            }
        }
        private void GetMediaFiles()
        {
            Initialize();
            SelectedType = 0;
            if (!string.IsNullOrEmpty(Link))
            {
                var youtube = new YouTube();
                var playlist = youtube.GetMediaFiles(Link);
                if (playlist.Count > 0)
                {
                    foreach (var p in playlist)
                    {
                        var media = new Media(p.Item1, p.Item2, p.Item3);
                        MediaFiles.Add(media);
                    }
                }
                else
                {
                    LinkError();
                    FilesVisibility = Visibility.Hidden;
                }
            }
        }
        private bool ValidPath()
        {
            if (Directory.Exists(Folder))
            {
                return true;
            }
            return false;
        }
        private void LinkError()
        {
            InformationForeground = new SolidColorBrush(Colors.Red);
            Information = "Please insert a valid youtube link!";
        }
        private void ClearInformation()
        {
            Information = "";
        }
        private void PathError()
        {
            InformationForeground = new SolidColorBrush(Colors.Red);
            Information = "Please add a valid download path!";
        }
        private void DownloadNothing()
        {
            InformationForeground = new SolidColorBrush(Colors.Red);
            Information = "Nothing Selected To Download!";
        }
        private void ShowStatus()
        {
            InformationForeground = new SolidColorBrush(Colors.Black);
            Information = downloaded + " out of " + selected;
        }
        private void ClearOutput()
        {
            Output = "";
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
        private void CalculatePercentage() => Percentage = (downloaded * 100 / selected).ToString() + " %";
        private void ChangeTypes()
        {
            foreach (var m in MediaFiles)
            {
                m.TypeIndex = SelectedType;
            }
        }
        private void ChangeQualities()
        {
            foreach (var m in MediaFiles)
            {
                m.QualityIndex = SelectedQuality;
            }
        }

        // On Property Changed Interface
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace YouTubeDownloader.Downloader
{
    internal class YouTube
    {
        private YoutubeClient client;
        private string pythonDll;
        private string downloderPy;
        private string downloderPyV2;
        internal YouTube()
        {
            client = new YoutubeClient();
            pythonDll = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Python\Python312\python312.dll";
            downloderPy = Path.Combine(Environment.CurrentDirectory.ToString(),"Files", "Python.txt");
            downloderPyV2 = Path.Combine(Environment.CurrentDirectory.ToString(), "Python.txt");
        }
        private string ReturnPythonScript()
        {
            if (!File.Exists(downloderPy))
            {
                return File.ReadAllText(downloderPyV2);
            }
                return File.ReadAllText(downloderPy);
        }
        private string SetFileFinalName(string path, string name, int duplicate)
        {
            if (File.Exists(Path.Combine(path, "DUPLICATE[" + duplicate + "] " + name)))
            {
                return SetFileFinalName(path, name, ++duplicate);
            }
            return "DUPLICATE[" + duplicate + "] " + name;
        }
        public string DownloadVideo(string link, string name, string downloadPath, string quality)
        {
            var output = "ERROR: Python Script Did Not Execute!" + Environment.NewLine;
            name = name + ".mp4";
            downloadPath = downloadPath.Replace("\\", "\\\\");
            if (File.Exists(Path.Combine(downloadPath, name)))
            {
                name = SetFileFinalName(downloadPath, name, 1);
            }

            Runtime.PythonDLL = pythonDll;
            PythonEngine.Initialize();
            var scope = Py.CreateScope();
            using (Py.GIL())
            {
                var text = ReturnPythonScript();
                scope.Exec(text);
                var downloadVideo = "DownloadVideo(\"" + link + "\",\"" + downloadPath + "\",\"" + name + "\",\"" + quality + "\")";
                output = scope.Eval(downloadVideo).ToString();
            }
            PythonEngine.Shutdown();
            return output;
        }

        public string DownloadAudio(string link, string name, string downloadPath, string quality)
        {
            var output = "ERROR: Python Script Did Not Execute!" + Environment.NewLine;
            name = name + ".mp3";
            downloadPath = downloadPath.Replace("\\", "\\\\");
            if (File.Exists(Path.Combine(downloadPath, name)))
            {
                name = SetFileFinalName(downloadPath, name, 1);
            }

            Runtime.PythonDLL = pythonDll;
            PythonEngine.Initialize();
            var scope = Py.CreateScope();
            using (Py.GIL())
            {
                var text = ReturnPythonScript();
                scope.Exec(text);
                var downloadAudio = "DownloadAudio(\"" + link + "\",\"" + downloadPath + "\",\"" + name + "\",\"" + quality + "\")";
                output = scope.Eval(downloadAudio).ToString();
            }
            PythonEngine.Shutdown();
            return output;
        }

        public string DownloadInvalidType(string link, string name)
        {
            var output = "ERROR: Invalid Download Type for " + name;
            return output;
        }
        public List<Tuple<string, string, string>> GetMediaFiles(string link)
        {
            var list = new List<Tuple<string, string, string>>();
            if (link.StartsWith("https://www.youtube.com/watch"))
            {
                var video = GetVideo(link);
                list.Add(video);
            }
            else if (link.StartsWith("https://www.youtube.com/playlist"))
            {
                list = GetPlaylist(link);
            }
            return list;
        }
        private List<Tuple<string, string, string>> GetPlaylist(string link)
        {
            var list = new List<Tuple<string, string, string>>();
            var playlist = Task.Run(async () => await client.Playlists.GetVideosAsync(link));
            foreach (var video in playlist.Result)
            {
                var linkvideo = string.Join("_", video.Url);
                var name = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));
                var thumbnail = string.Join("_", video.Thumbnails[video.Thumbnails.Count - 1].Url);
                var tuple = new Tuple<string, string, string>(linkvideo, name, thumbnail);
                list.Add(tuple);
            }
            return list;
        }
        private Tuple<string, string, string> GetVideo(string link)
        {
            var video = Task.Run(async () => await client.Videos.GetAsync(link));
            var name = string.Join("_", video.Result.Title.Split(Path.GetInvalidFileNameChars()));
            var thumbnail = string.Join("_", video.Result.Thumbnails[video.Result.Thumbnails.Count - 1].Url);
            var tuple = new Tuple<string, string, string>(link, name, thumbnail);
            return tuple;
        }
    }
}

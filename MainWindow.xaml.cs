using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using YouTubeDownloader.Downloader;

namespace YouTubeDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel viewModel;
        public MainWindow()
        {
            viewModel = new ViewModel();
            DataContext = viewModel;
        }
        private void PasteLink_Click(object sender, RoutedEventArgs e)
        {
            viewModel.PasteLink();
        }
        private void ClearLink_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ClearLink();
        }
        private void Download_Click(object sender, RoutedEventArgs e)
        {
            viewModel.DownloadAll();
        }
        private void Before_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Before();
        }
        private void After_Click(object sender, RoutedEventArgs e)
        {
            viewModel.After();
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Refresh();
        }
        private void PasteNameBeforeAfter_Click(object sender, RoutedEventArgs e)
        {
            viewModel.PasteNameBeforeAfterText();
        }

        private void ClearNameBeforeAfter_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ClearNameBeforeAfterText();
        }

        private void OpenFolderDialog_Click(object sender, RoutedEventArgs e)
        {

            var openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.ShowDialog();
            if (!string.IsNullOrEmpty(openFolderDialog.SelectedPath))
            {
                viewModel.Folder = openFolderDialog.SelectedPath;
            }
        }
        private void LinkOpen(object sender, RoutedEventArgs e)
        {
            var link = (sender as System.Windows.Controls.Image).Tag.ToString();
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
        }
        private void CheckFile_Click(object sender, RoutedEventArgs e)
        {
            var link = (sender as System.Windows.Controls.CheckBox).Tag.ToString();
            viewModel.CheckFile(link);
        }
        private void DownloadFile_Click(object sender, RoutedEventArgs e)
        {
            var link = (sender as System.Windows.Controls.Button).Tag.ToString();
            viewModel.Download(link);
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Cancel();
        }
        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}

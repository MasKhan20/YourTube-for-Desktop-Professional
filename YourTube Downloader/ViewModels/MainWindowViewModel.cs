using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using VideoLibrary;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows;
using YourTube_Downloader.Services;
using YourTube_Downloader.Models;
using System.Diagnostics;
using static YourTube_Downloader.Properties.Settings;
using YourTube_Downloader.Views;

namespace YourTube_Downloader.ViewModels
{
    public class MainWindowViewModel : ObservableObject, IViewAccessingViewModel<UIElements>
    {
        #region Command Assignments
        public ICommand OpenCommand => new RelayCommand(OpenMenu_Command);
        public ICommand SettingsWindowCommand => new RelayCommand(SettingsWindowMenu_Command);
        public ICommand HelpCommand => new RelayCommand(HelpMenu_Command);
        public ICommand BrowseCommand => new RelayCommand(BrowseButton_Command);
        public ICommand ViewLogCommand => new RelayCommand(ViewLogButton_Command);

        public ICommand DownloadCommand => new RelayCommand(async () => await Task.Run(() => Download(
            ViewProperty.YTVideoURL, 
            ViewProperty.YTAudioButton,
            ViewProperty.YTLocation,
            
            ViewProperty.YTStatusLabel,
            ViewProperty.YTProgressBar,
            ViewProperty.YTDownloadButton)));
        #endregion


        #region Binding Properties
        private string _uIBGColor;
        public string UIBGColor
        {
            get { return _uIBGColor; }
            set { Set(() => UIBGColor, ref _uIBGColor, value); }
        }

        private string _uIHeaderColor;
        public string UIHeaderColor
        {
            get { return _uIHeaderColor; }
            set { Set(() => UIHeaderColor, ref _uIHeaderColor, value); }
        }

        private string _downloadDir;
        public string DownloadDir
        {
            get { return _downloadDir; }
            set { Set(() => DownloadDir, ref _downloadDir, value); }
        }

        private bool _multipleLinks;
        public bool MultipleLinks
        {
            get { return _multipleLinks; }
            set
            {
                Set(() => MultipleLinks, ref _multipleLinks, value);
                if (MultipleLinks == true)
                {
                    ViewProperty.YTVideoURL.TextWrapping = TextWrapping.Wrap;
                    ViewProperty.YTVideoURL.AcceptsReturn = true;
                    ViewProperty.YTVideoURL.Height = 138;
                }
                else
                {
                    ViewProperty.YTVideoURL.TextWrapping = TextWrapping.NoWrap;
                    ViewProperty.YTVideoURL.AcceptsReturn = false;
                    ViewProperty.YTVideoURL.Height = 23;
                }
            }
        }

        private string _downloadPercent;
        public string DownloadPercent
        {
            get { return _downloadPercent; }
            set { Set(() => DownloadPercent, ref _downloadPercent, value); }
        }

        public UIElements ViewProperty { get; set; }
        #endregion


        public MainWindowViewModel()
        {
            UIBGColor = Default.BackGroundColor;
            UIHeaderColor = Default.HeaderColor;

            DownloadDir
                    = Default.UseAppDir
                    ? ConstModel.AppDir
                    : Default.UserSaveDir;

            Default.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UIHeaderColor = Default.HeaderColor;
            UIBGColor = Default.BackGroundColor;

            if (Default.UserSaveDir == "")
            {
                Default.UserSaveDir
                    = Default.UseAppDir
                    ? ConstModel.AppDir
                    : Default.UserSaveDir;
            }
            else
            {
                DownloadDir 
                    = Default.UseAppDir
                    ? ConstModel.AppDir
                    : Default.UserSaveDir;
            }
        }

        #region Video/Audio Download Methods
        public async Task Download(TextBox yTVideoURL, RadioButton yTAudioButton, TextBox yTLocation, 
                                   Label yTStatusLabel, ProgressBar yTProgressBar, Button yTDownloadButton)
        {
            var link = yTVideoURL.Text;
            var location = yTLocation.Text;
            var format = (yTAudioButton.IsChecked.Value) ? "Audio" : "Video";

            if (!IsDirValid(location))
            {
                return;
            }

            /* Download */
            yTDownloadButton.IsEnabled = false;
            yTProgressBar.Value = 0;
            yTProgressBar.IsIndeterminate = true;

            try
            {
                var extension = (format == "Audio") ? "mp3" : "mp4";
                await Task.Run(() => ExecuteDownload(yTStatusLabel, yTProgressBar, yTDownloadButton, link, format, extension, location));
                MessageBox.Show($"Downloaded {format.ToLower()} to:\n{location}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                CancelDownload(yTStatusLabel, yTProgressBar, yTDownloadButton, "An error occurred while sending the request. ");
                return;
            }
            catch (System.InvalidOperationException)
            {
                CancelDownload(yTStatusLabel, yTProgressBar, yTDownloadButton, "Invalid YouTube video URL");
                return;
            }
            catch (Exception exc)
            {
                CancelDownload(yTStatusLabel, yTProgressBar, yTDownloadButton, exc.ToString());
                return;
            }

        }

        private static void CancelDownload(Label yTStatusLabel, ProgressBar yTProgressBar, Button yTDownloadButton, string errorMsg)
        {
            yTStatusLabel.Content = "Failed";
            yTDownloadButton.IsEnabled = true;
            yTProgressBar.IsIndeterminate = false;
            yTProgressBar.Value = 0;
            MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static bool IsDirValid( string location)
        {
            /* Validate download directory */
            if (location.Length <= 0 || location == null)
            {
                MessageBox.Show("No download location specified", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void ExecuteDownload(Label yTStatusLabel, ProgressBar yTProgressBar, Button yTDownloadButton, string link, string format, string extension, string location)
        {
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(link);

            var fullFileName = System.IO.Path.Combine(location, RemoveIllegalPathCharacters(video.FullName));

            yTStatusLabel.Content = "Downloading content";

            byte[] videoBytes = video.GetBytes();
            var videoFileByteLength = videoBytes.Length;

            using (FileStream dFile = File.OpenWrite(fullFileName))
            {
                dFile.Write(videoBytes, 0, videoFileByteLength);
            }

            yTStatusLabel.Content = $"Converting to {extension}";

            var infile = new MediaToolkit.Model.MediaFile { Filename = fullFileName };
            var outfile = new MediaToolkit.Model.MediaFile { Filename = $"{System.IO.Path.Combine(location, RemoveIllegalPathCharacters(video.Title.Replace(" - YouTube", "")))}.{extension}" };

            if (!fullFileName.ToLower().EndsWith(extension))
            {
                Convert(infile, outfile);
            }

            yTDownloadButton.IsEnabled = true;
            yTProgressBar.Value = 100;
            yTStatusLabel.Content = $"Done! Downloaded: {Converter.GetSize(videoBytes)}";

            YourTube_Downloader.Setting.AppSettings.AddDownloadUsageBytes(videoBytes);
        }

        public void Convert(MediaToolkit.Model.MediaFile inFile, MediaToolkit.Model.MediaFile outFile)
        {
            using (var engine = new MediaToolkit.Engine())
            {
                ViewProperty.YTProgressBar.IsIndeterminate = false;
                //Subscribe
                engine.ConvertProgressEvent += Engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent += Engine_ConversionCompleteEvent;

                Task.Run(() => engine.Convert(inFile, outFile));
                if (File.Exists(inFile.Filename))
                {
                    File.Delete(inFile.Filename);
                }
                
                //Unsubscribe
                engine.ConvertProgressEvent -= Engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent -= Engine_ConversionCompleteEvent;
            }
        }

        private void Engine_ConversionCompleteEvent(object sender, MediaToolkit.ConversionCompleteEventArgs e)
        {
            var value = (e.ProcessedDuration.TotalSeconds / e.TotalDuration.TotalSeconds * 100);
            ViewProperty.YTProgressBar.Value = value;
            DownloadPercent = $"{value} % Complete";
        }

        private void Engine_ConvertProgressEvent(object sender, MediaToolkit.ConvertProgressEventArgs e)
        {
            var value = (e.ProcessedDuration.TotalSeconds / e.TotalDuration.TotalSeconds * 100);
            ViewProperty.YTProgressBar.Value = value;
            DownloadPercent = $"{value} % Complete";
        }

        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }
        #endregion

        #region Commands
        private void OpenMenu_Command()
        {
            if (Default.UseAppDir)
            {
                Process.Start(ConstModel.AppDir);
            }
            else
            {
                if (Directory.Exists(ViewProperty.YTLocation.Text))
                {
                    Process.Start(ViewProperty.YTLocation.Text);
                }
                else
                {
                    Process.Start(Default.UserSaveDir);
                }
            }
        }

        private void SettingsWindowMenu_Command()
        {
            var settingsDialog = new SettingsWindow();

            settingsDialog.ShowDialog();
        }

        private void HelpMenu_Command()
        {
            MessageBox.Show("YourTube for Desktop v1\nHelp not currently available", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BrowseButton_Command()
        {
            string dir;

            if (Directory.Exists(DownloadDir))
            {
                dir = FolderExtention.GetFolder(DownloadDir);
            }
            else
            {
                dir = FolderExtention.GetFolder(ConstModel.AppDir);
            }

            if (dir == null)
            {
                return;
            }

            DownloadDir = dir;
        }

        private void ViewLogButton_Command()
        {
            MessageBox.Show("This feature is yet to be implemented", "Stay tuned", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion
    }
}

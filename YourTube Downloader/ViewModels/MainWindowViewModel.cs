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

        public ICommand DownloadCommand => new RelayCommand(async () => await BeginDownload());
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

        #region

        private string _yTLink;
        /// <summary>
        /// YouTube link for video
        /// </summary>
        public string YTLink
        {
            get { return _yTLink; }
            set { Set(() => YTLink, ref _yTLink, value); }
        }

        private bool _yTIsVideo = true;
        /// <summary>
        /// Checks if to download video, false is audio
        /// </summary>
        public bool YTIsVideo
        {
            get { return _yTIsVideo; }
            set { Set(() => YTIsVideo, ref _yTIsVideo, value); }
        }

        private string _yTStatusLabel;
        /// <summary>
        /// Displays the current state of the download
        /// </summary>
        public string YTStatusLabel
        {
            get { return _yTStatusLabel; }
            set { Set(() => YTStatusLabel, ref _yTStatusLabel, value); }
        }

        private bool _isDownloadEnabled = true;
        /// <summary>
        /// Enables/disables the download button
        /// </summary>
        public bool IsDownloadEnabled
        {
            get { return _isDownloadEnabled; }
            set { Set(() => IsDownloadEnabled, ref _isDownloadEnabled, value); }
        }

        private double _progressValue;
        /// <summary>
        /// Sets the percentage loaded of the progress bar
        /// </summary>
        public double ProgressValue
        {
            get { return _progressValue; }
            set { Set(() => ProgressValue, ref _progressValue, value); }
        }
        #endregion

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

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(() => IsLoading, ref _isLoading, value); }
        }

        private string _loadedPercent;
        public string LoadedPercent
        {
            get { return _loadedPercent; }
            set { Set(() => LoadedPercent, ref _loadedPercent, value); }
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

#if DEBUG
            YTLink = "https://www.youtube.com/watch?v=ciaEisQ6cdE";
#endif
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
        private async Task BeginDownload()
        {
            var links = YTLink.Split('\n');
            var downloadNum = MultipleLinks ? links.Length :  1;

            if (MultipleLinks == false && links.Length > 1)
            {
                var result =  MessageBox.Show("You have not enabled multiple links, but their are multiple lines in the text box, \nonly the first link will be downloaded. \nAre you sure you want to continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            int i = 0;

            foreach (var link in links)
            {
                if (i == downloadNum)
                    return;
                if (link == string.Empty)
                    continue;

                YTStatusLabel = $"Downloading link: {i + 1} of {downloadNum}";

                await Download(link);

                i++;
            }

            YTStatusLabel = $"Finished downloading videos: {i} of {downloadNum}";
        }

        private async Task Download(string link)
        {
            //var link = YTLink;
            var location = DownloadDir;
            var format = YTIsVideo ? "Video" : "Audio";

            if (string.IsNullOrEmpty(link))
            {
                MessageBox.Show("No YouTube URL entered", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!IsDirValid(location))
            {
                return;
            }
            if (!Directory.Exists(location))
            {
                MessageBox.Show("Invalid directory", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            /* Download */
            IsDownloadEnabled = false;
            IsLoading = true;

            try
            {
                var extension = (format == "Audio") ? "mp3" : "mp4";
                await ExecuteDownload(link, format, extension, location);
                MessageBox.Show($"Downloaded {format.ToLower()} to:\n{location}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                CancelDownload("An error occurred while sending the request. ");
                return;
            }
            catch (InvalidOperationException)
            {
                CancelDownload("Invalid YouTube video URL");
                return;
            }
            catch (Exception exc)
            {
                CancelDownload(exc.Message.ToString());
                return;
            }

        }

        private void CancelDownload(string errorMsg, string log = null)
        {
            YTStatusLabel = "Failed";
            IsDownloadEnabled = true;
            IsLoading = false;
            ProgressValue = 0;
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

        private async Task ExecuteDownload(string link, string format, string extension, string location)
        {
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(link);

            var fullFileName = System.IO.Path.Combine(location, RemoveIllegalPathCharacters(video.FullName));

            LoadedPercent = "";

            byte[] videoBytes = await video.GetBytesAsync();
            var videoFileByteLength = videoBytes.Length;

            using (FileStream dFile = File.OpenWrite(fullFileName))
            {
                await dFile.WriteAsync(videoBytes, 0, videoFileByteLength);
            }


            var infile = new MediaToolkit.Model.MediaFile { Filename = fullFileName };
            var outfile = new MediaToolkit.Model.MediaFile { Filename = $"{System.IO.Path.Combine(location, RemoveIllegalPathCharacters(video.Title.Replace(" - YouTube", "")))}.{extension}" };

            if (!fullFileName.ToLower().EndsWith(extension))
            {
                YTStatusLabel = $"Converting to {extension}";
                await Task.Run(() => Convert(infile, outfile));
            }

            IsDownloadEnabled = true;
            IsLoading = false;
            ProgressValue = 100;
            YTStatusLabel = $"Done! Downloaded: {Converter.GetSize(videoBytes)}";

            YourTube_Downloader.Setting.AppSettings.AddDownloadUsageBytes(videoBytes);
        }

        public void Convert(MediaToolkit.Model.MediaFile inFile, MediaToolkit.Model.MediaFile outFile)
        {
            using (var engine = new MediaToolkit.Engine())
            {
                IsLoading = false;
                //Subscribe
                engine.ConvertProgressEvent += Engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent += Engine_ConversionCompleteEvent;

                LoadedPercent = "Converting ...";
                engine.Convert(inFile, outFile);
                if (File.Exists(inFile.Filename))
                {
                    File.Delete(inFile.Filename);
                }
                LoadedPercent = "Conversion complete";

                //Unsubscribe
                engine.ConvertProgressEvent -= Engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent -= Engine_ConversionCompleteEvent;
            }
        }

        private void Engine_ConversionCompleteEvent(object sender, MediaToolkit.ConversionCompleteEventArgs e)
        {
            var value = (e.ProcessedDuration.TotalSeconds / e.TotalDuration.TotalSeconds * 100);
            ProgressValue = value;
            LoadedPercent = $"{(int)value} % Complete";
        }

        private void Engine_ConvertProgressEvent(object sender, MediaToolkit.ConvertProgressEventArgs e)
        {
            var value = (e.ProcessedDuration.TotalSeconds / e.TotalDuration.TotalSeconds * 100);
            ProgressValue = value;
            LoadedPercent = $"{(int)value} % Complete";
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

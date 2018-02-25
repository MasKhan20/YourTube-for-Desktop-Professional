using System;
using System.Windows;
using System.Windows.Controls;
using YourTube_Downloader.ViewModels;
using YourTube_Downloader.Views;
using YourTube_Downloader.Setting;

namespace YourTube_Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string link;
        public string format;
        public string location;
        public TextBox logger;

        public LogWindow loggerWindow;

        public MainWindow()
        {
            AppSettings appSettings = new AppSettings();

            InitializeComponent();
            
            DataContext = new MainWindowViewModel
            {
                ViewProperty = new UIElements()
                {
                    YTVideoURL = YTVideoID,
                    YTAudioButton = YTAudioFormat,
                    YTLocation = YTLocation,

                    YTDownloadButton = DownloadButton,
                    YTProgressBar = YTProgress,
                    YTStatusLabel = StatusLabel
                }
            };

            /* Initialize variables */
            this.SizeToContent = SizeToContent.Height;

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Properties.Settings.Default.Save();
            Application.Current.Shutdown();
        }

        public static void AddLog(TextBox textbox , string value)
        {
            if (textbox == null)
            {
                return;
            }
            textbox.Text.Insert(textbox.Text.Length, value);
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

using System;
using System.Windows;
using YourTube_Downloader.ViewModels;

namespace YourTube_Downloader.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        private static SettingsWindow _instance = new SettingsWindow();
        public static SettingsWindow Instance { get { return _instance; } }

        public SettingsWindow()
        {
            InitializeComponent();
            var viewModel = new SettingsViewModel(this)
            {
                ViewProperty = new SettingElements()
                {
                    HeaderCombo = HeaderCombo,
                    BGCombo = BGCombo,
                    DownloadDirBox = DownloadDir,
                    UseCurrDirButton = AppDirButton
                }
            };

            DataContext = viewModel;

            switch (Properties.Settings.Default.UseAppDir)
            {
                case true:
                    AppDirButton.IsChecked = true;
                    break;
                default:
                    SpecDirButton.IsChecked = true;
                    break;
            }

            
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            //Properties.Settings.Default.Save();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

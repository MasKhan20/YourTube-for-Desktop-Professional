using System;
using System.Windows;
using YourTube_Downloader.Properties;
using System.IO;
using static YourTube_Downloader.Properties.Settings;

namespace YourTube_Downloader.Setting
{
    public class AppSettings
    {
        public AppSettings()
        {
            Settings.Default.UserSaveDir = 
                Directory.Exists(Settings.Default.UserSaveDir)
                ? Settings.Default.UserSaveDir
                : Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                    "YourTube", "Downloads");

            if (!Directory.Exists(Default.UserSaveDir))
            {
                try
                {
                    Directory.CreateDirectory(Default.UserSaveDir);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Cannot create Downloads directory", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public static void AddDownloadUsageBytes(byte[] videoBytes)
        {
            Settings.Default.DownloadedBytes += videoBytes.Length;
        }

        public static void UpdateColors(string headerColor, string bGColor)
        {
            Properties.Settings.Default.HeaderColor = headerColor;
            Properties.Settings.Default.BackGroundColor = bGColor;
        }
    }
}

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YourTube_Downloader.Models;
using YourTube_Downloader.Services;
using YourTube_Downloader.Views;

namespace YourTube_Downloader.ViewModels
{
    public class SettingsViewModel : ObservableObject, IViewAccessingViewModel<SettingElements>
    {
        public ICommand SaveAndCloseCommand => new RelayCommand(SaveAndCloseWindow);
        public ICommand ApplyCommand => new RelayCommand(ApplySettingsCommand);
        public ICommand ResetCommand => new RelayCommand(ResetAllButton_Command);
        public ICommand DownloadDirCommand => new RelayCommand(GetUserSaveDir);

        #region Properties
        /// <summary>
        /// List of all available colors
        /// </summary>
        private List<string> _colorList;
        public List<string> ColorList
        {
            get { return _colorList; }
            set { Set(() => ColorList, ref _colorList, value); }
        }

        public SettingElements ViewProperty { get; set; }

        private string _nowBGColor;
        public string NowBGColor
        {
            get { return _nowBGColor; }
            set { Set(() => NowBGColor, ref _nowBGColor, value); }
        }
        public string _nowHeaderColor;
        public string NowHeaderColor
        {
            get { return _nowHeaderColor; }
            set { Set(() => NowHeaderColor, ref _nowHeaderColor, value); }
        }

        private string _dataUsageBytes;
        public string DataUsageBytes
        {
            get { return _dataUsageBytes; }
            set { Set(() => DataUsageBytes, ref _dataUsageBytes, value); }
        }

        private string _appDir;
        public string AppDir
        {
            get { return _appDir; }
            set { Set(() => AppDir, ref _appDir, value); }
        }

        private string _specDir;
        public string SpecDir
        {
            get { return _specDir; }
            set { Set(() => SpecDir, ref _specDir, value); }
        }
        #endregion

        /// <summary>
        /// Get rid of this as soon as possible, it is against the MVVM practices
        /// </summary>
        public SettingsWindow SetWin;

        public SettingsViewModel(SettingsWindow settingsWindow)
        {
            Type colorsType = typeof(System.Windows.Media.Colors);
            PropertyInfo[] colorsTypePropertyInfos = colorsType.GetProperties(BindingFlags.Public | BindingFlags.Static);
            
            ColorList = new List<string>();
            foreach (PropertyInfo colorsTypePropertyInfo in colorsTypePropertyInfos)
            {
                ColorList.Add(colorsTypePropertyInfo.Name);
            }

            SetWin = settingsWindow;

            NowHeaderColor = Properties.Settings.Default.HeaderColor;
            NowBGColor = Properties.Settings.Default.BackGroundColor;

            DataUsageBytes = Converter.GetSizeText(Properties.Settings.Default.DownloadedBytes) + "(s) of content downloaded";
            AppDir = ConstModel.AppDir;
            SpecDir = Properties.Settings.Default.UserSaveDir;
        }

        /* Commands */
        public void GetUserSaveDir()
        {
            TextBox textBox = ViewProperty.DownloadDirBox;
            string dir = FolderExtention.GetFolder(textBox.Text);

            if (dir == null)
            {
                return;
            }

            textBox.Text = dir;
            Properties.Settings.Default.UserSaveDir = dir;
        }

        /// <summary>
        /// Saves all user editable settings and closes window. 
        /// Currently works against MVVM, fix as soon as possible!
        /// </summary>
        public void SaveAndCloseWindow()
        {
            ApplySettingsCommand();

            SetWin.Close();
        }

        /// <summary>
        /// Saves all user editable settings
        /// </summary>
        private void ApplySettingsCommand()
        {
            /* Update colors */
            Properties.Settings.Default.HeaderColor = ViewProperty.HeaderCombo.SelectedValue.ToString();
            Properties.Settings.Default.BackGroundColor = ViewProperty.BGCombo.SelectedValue.ToString();

            /* Set whether or not to use current directory */
            Properties.Settings.Default.UseAppDir = 
                (bool)ViewProperty.UseCurrDirButton.IsChecked ? true : false;

            /* Update download directory */
            string newDir = ViewProperty.DownloadDirBox.Text;

            if (Directory.Exists(newDir))
                Properties.Settings.Default.UserSaveDir = newDir;
            else
                MessageBox.Show("Selected download directory does not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error); 

            Properties.Settings.Default.Save();
        }

        private void ResetAllButton_Command()
        {
            var result = MessageBox.Show("All settings will be reset", "Are you sure?",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.Reset();
                MessageBox.Show("All configuration settings have been reset", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Operation Canceled", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}

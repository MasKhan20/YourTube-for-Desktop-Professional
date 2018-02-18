using System.Windows.Controls;

namespace YourTube_Downloader.ViewModels
{
    internal interface IViewAccessingViewModel<T>
    {
        T ViewProperty { get; set; }
    }

    public class UIElements
    {
        public TextBox YTVideoURL { get; set; }
        public RadioButton YTAudioButton { get; set; }
        public TextBox YTLocation { get; set; }

        public Label YTStatusLabel { get; set; }
        public ProgressBar YTProgressBar { get; set; }
        public Button YTDownloadButton { get; set; }
    }

    public class SettingElements
    {
        public ComboBox HeaderCombo { get; set; }
        public ComboBox BGCombo { get; set; }

        public TextBox DownloadDirBox { get; set; }

        public RadioButton UseCurrDirButton { get; set; }
    }
}
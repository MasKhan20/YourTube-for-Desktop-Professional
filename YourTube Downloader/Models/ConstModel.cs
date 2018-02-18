using System;
using System.IO;

namespace YourTube_Downloader.Models
{
    public static class ConstModel
    {
        public static string AppDir
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YourTube", "Downloads");
            }
        }
    }
}

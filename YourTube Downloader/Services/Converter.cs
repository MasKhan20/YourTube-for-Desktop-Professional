using System;

namespace YourTube_Downloader.Services
{
    public class Converter
    {
        public static string GetSize(byte[] bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes.Length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        public static string GetSize(double bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }

            return String.Format("{0:0.##} {1}", bytes, sizes[order]);
        }

        public static string GetSizeText(double bytes)
        {
            string[] sizes = { "Bytes", "Kilobyte", "Megabyte", "Gigabyte", "Terabyte", "Petabyte" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }

            return String.Format("{0:0.##} {1}", bytes, sizes[order]);
        }
    }
}

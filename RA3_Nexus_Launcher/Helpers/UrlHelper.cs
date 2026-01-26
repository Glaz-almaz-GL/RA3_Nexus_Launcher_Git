using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RA3_Nexus_Launcher.Helpers
{
    public static class UrlHelper
    {
        /// <summary>
        /// Открывает указанный URL-адрес в браузере по умолчанию операционной системы.
        /// </summary>
        /// <param name="url">URL-адрес для открытия (например, "https://www.example.com").</param>
        public static void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL сannot be null or empty.", nameof(url));
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true // Важно для открытия URL через системный браузер
            });
        }
    }
}

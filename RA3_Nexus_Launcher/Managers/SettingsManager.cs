using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Helpers;
using RA3_Nexus_Launcher.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace RA3_Nexus_Launcher.Managers
{
    public static class SettingsManager
    {
        public static LauncherSettings CurrentSettings { get; private set; } = LoadSettings();

        private static JsonSerializerOptions? _jsonOptions; // Сделали nullable

        private static JsonSerializerOptions GetJsonOptions() // Метод для получения/инициализации
        {
            return _jsonOptions ??= new JsonSerializerOptions { WriteIndented = true };
        }

        public static LauncherSettings LoadSettings()
        {
            string settingsPath = PathConstants.LauncherSettingsPath;

            if (!Directory.Exists(PathConstants.LauncherSettingsFolder) || !File.Exists(settingsPath))
            {
                Debug.Write($"Settings file not found: {settingsPath}");

                if (!Directory.Exists(PathConstants.LauncherSettingsFolder))
                {
                    Directory.CreateDirectory(PathConstants.LauncherSettingsFolder);
                }

                LauncherSettings defaultSettings = new();
                SaveSettings(defaultSettings); // Теперь вызовет GetJsonOptions()
                return defaultSettings;
            }

            try
            {
                string json = File.ReadAllText(settingsPath);
                LauncherSettings? loadedSettings = JsonSerializer.Deserialize<LauncherSettings>(json, GetJsonOptions()); // Используем метод
                return loadedSettings ?? new LauncherSettings();
            }
            catch (JsonException ex)
            {
                NotificationHelpers.ShowError("Error reading settings file. Default settings are being used.", $"{ex.Message} {ex.InnerException}", TimeSpan.FromSeconds(5));
                return new LauncherSettings();
            }
            catch (IOException ex)
            {
                NotificationHelpers.ShowError("Error accessing settings file. Default settings are being used.", $"{ex.Message} {ex.InnerException}", TimeSpan.FromSeconds(5));
                return new LauncherSettings();
            }
        }

        public static void SaveSettings(LauncherSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            if (!Directory.Exists(PathConstants.LauncherSettingsFolder))
            {
                Directory.CreateDirectory(PathConstants.LauncherSettingsFolder);
            }

            try
            {
                // Используем метод для получения инициализированного экземпляра
                string json = JsonSerializer.Serialize(settings, GetJsonOptions());
                File.WriteAllText(PathConstants.LauncherSettingsPath, json);
            }
            catch (IOException ex)
            {
                NotificationHelpers.ShowError("Error saving settings file", $"{ex.Message} {ex.InnerException}", TimeSpan.FromSeconds(5));
                throw;
            }
        }

        public static void ReloadSettings()
        {
            CurrentSettings = LoadSettings();
        }

        public static void SaveCurrentSettings()
        {
            SaveSettings(CurrentSettings);
        }
    }
}

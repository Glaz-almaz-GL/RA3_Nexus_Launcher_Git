using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Models;
using System;
using System.IO;
using System.Text.Json;

namespace RA3_Nexus_Launcher.Managers
{
    public static class SettingsManager
    {
        public static Settings CurrentSettings { get; private set; } = LoadSettings();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public static Settings LoadSettings()
        {
            string settingsPath = PathConstants.LauncherSettingsPath;

            if (!Directory.Exists(PathConstants.LauncherFolder) || !File.Exists(settingsPath))
            {
                if (!Directory.Exists(PathConstants.LauncherFolder))
                {
                    Directory.CreateDirectory(PathConstants.LauncherFolder);
                }

                Settings defaultSettings = new();
                SaveSettings(defaultSettings);
                return defaultSettings;
            }

            try
            {
                string json = File.ReadAllText(settingsPath);
                Settings? loadedSettings = JsonSerializer.Deserialize<Settings>(json, _jsonOptions);
                return loadedSettings ?? new Settings();
            }
            catch (JsonException ex)
            {
                // Ошибка десериализации (например, файл повреждён)
                Console.WriteLine($"Ошибка чтения файла настроек: {ex.Message}. Используются настройки по умолчанию.");
                return new Settings();
            }
            catch (IOException ex)
            {
                // Ошибка ввода-вывода (например, файл заблокирован)
                Console.WriteLine($"Ошибка доступа к файлу настроек: {ex.Message}. Используются настройки по умолчанию.");
                return new Settings();
            }
        }

        public static void SaveSettings(Settings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            // Убедимся, что папка существует перед сохранением
            if (!Directory.Exists(PathConstants.LauncherFolder))
            {
                Directory.CreateDirectory(PathConstants.LauncherFolder);
            }

            try
            {
                string json = JsonSerializer.Serialize(settings, _jsonOptions);
                File.WriteAllText(PathConstants.LauncherSettingsPath, json);
            }
            catch (IOException ex)
            {
                // Ошибка ввода-вывода (например, нет прав на запись)
                Console.WriteLine($"Ошибка сохранения файла настроек: {ex.Message}");
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

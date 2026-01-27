using RA3_Nexus_Launcher.Models;
using RA3_Nexus_Launcher.Helpers; // Добавьте этот using для NotificationHelpers
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace RA3_Nexus_Launcher.Helpers
{
    public static class IniFileHelper
    {
        private const string IniFileNotFoundMsg = "INI File Not Found";

        public static (double? AmbientVolume, double? MovieVolume, double? MusicVolume, double? SFXVolume, double? VoiceVolume) ReadAudioVolumes(string filePath)
        {
            if (!File.Exists(filePath))
            {
                NotificationHelpers.ShowError(IniFileNotFoundMsg, $"File not found: {filePath}", TimeSpan.FromSeconds(5));
                // Возвращаем кортеж с null значениями, если файл не найден
                return (null, null, null, null, null);
            }

            List<string> lines = [.. File.ReadAllLines(filePath)];

            double? ambientVolume = null;
            double? movieVolume = null;
            double? musicVolume = null;
            double? sfxVolume = null;
            double? voiceVolume = null; // Новое поле

            foreach (string line in lines)
            {
                string trimmedLine = line.TrimStart();

                ambientVolume = TryParseDoubleValue(trimmedLine, "AmbientVolume =", ambientVolume);
                movieVolume = TryParseDoubleValue(trimmedLine, "MovieVolume =", movieVolume);
                musicVolume = TryParseDoubleValue(trimmedLine, "MusicVolume =", musicVolume);
                sfxVolume = TryParseDoubleValue(trimmedLine, "SFXVolume =", sfxVolume);
                voiceVolume = TryParseDoubleValue(trimmedLine, "VoiceVolume =", voiceVolume);
            }

            return (ambientVolume, movieVolume, musicVolume, sfxVolume, voiceVolume);
        }

        private static double? TryParseDoubleValue(string line, string key, double? currentValue)
        {
            if (currentValue.HasValue || !line.StartsWith(key, StringComparison.OrdinalIgnoreCase))
            {
                return currentValue;
            }

            int equalsIndex = line.IndexOf('=');
            if (equalsIndex >= 0)
            {
                string valueStr = line[(equalsIndex + 1)..].Trim();
                if (double.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                {
                    return value;
                }
            }
            return currentValue;
        }

        public static (string? GameSpyIpAddress, string? IpAddress) ReadIpAddresses(string filePath)
        {
            if (!File.Exists(filePath))
            {
                NotificationHelpers.ShowError(IniFileNotFoundMsg, $"File not found: {filePath}", TimeSpan.FromSeconds(5));
                // Возвращаем кортеж с null значениями, если файл не найден
                return (null, null);
            }

            List<string> lines = [.. File.ReadAllLines(filePath)];

            string? gameSpyIpAddress = null;
            string? ipAddress = null;

            foreach (string line in lines)
            {
                string trimmedLine = line.TrimStart();

                gameSpyIpAddress = TryParseStringValue(trimmedLine, "GameSpyIPAddress =", gameSpyIpAddress);
                ipAddress = TryParseStringValue(trimmedLine, "IPAddress =", ipAddress);
            }

            return (gameSpyIpAddress, ipAddress);
        }

        private static string? TryParseStringValue(string line, string key, string? currentValue)
        {
            if (!string.IsNullOrEmpty(currentValue) || !line.StartsWith(key, StringComparison.OrdinalIgnoreCase))
            {
                return currentValue;
            }

            int equalsIndex = line.IndexOf('=');
            if (equalsIndex >= 0)
            {
                return line[(equalsIndex + 1)..].Trim();
            }
            return currentValue;
        }

        public static void WriteAudioVolumes(string filePath, double? ambientVolume = null, double? movieVolume = null, double? musicVolume = null, double? sfxVolume = null, double? voiceVolume = null)
        {
            if (!File.Exists(filePath))
            {
                NotificationHelpers.ShowError(IniFileNotFoundMsg, $"File not found: {filePath}", TimeSpan.FromSeconds(5));
                // Прерываем выполнение метода, если файл не найден
                return;
            }

            try
            {
                List<string> lines = [.. File.ReadAllLines(filePath)];
                ProcessAudioIniLines(lines, ambientVolume, movieVolume, musicVolume, sfxVolume, voiceVolume);
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("INI Write Error", $"Error writing to INI file: {filePath}. {ex.Message}", TimeSpan.FromSeconds(5));
                // Прерываем выполнение метода, если произошла ошибка ввода-вывода
                return;
            }
        }

        private static void ProcessAudioIniLines(List<string> lines, double? ambientVolume, double? movieVolume, double? musicVolume, double? sfxVolume, double? voiceVolume)
        {
            // Используем InvariantCulture для форматирования чисел
            var culture = CultureInfo.InvariantCulture;

            var audioValues = new[]
            {
                ("AmbientVolume =", ambientVolume),
                ("MovieVolume =", movieVolume),
                ("MusicVolume =", musicVolume),
                ("SFXVolume =", sfxVolume),
                ("VoiceVolume =", voiceVolume )
            };

            var foundFlags = new bool[audioValues.Length];

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].TrimStart();

                for (int j = 0; j < audioValues.Length; j++)
                {
                    if (audioValues[j].Item2.HasValue && line.StartsWith(audioValues[j].Item1, StringComparison.OrdinalIgnoreCase))
                    {
                        // Явно указываем культуру для форматирования числа
                        lines[i] = $"{audioValues[j].Item1} {audioValues[j].Item2!.Value.ToString("F6", culture)}";
                        foundFlags[j] = true;
                    }
                }
            }

            // Add missing values
            for (int j = 0; j < audioValues.Length; j++)
            {
                if (audioValues[j].Item2.HasValue && !foundFlags[j])
                {
                    // Явно указываем культуру и для добавляемых строк
                    lines.Add($"{audioValues[j].Item1} {audioValues[j].Item2!.Value.ToString("F6", culture)}");
                }
            }
        }

        public static void WriteIpAddresses(string filePath, string? gameSpyIpAddress, string? ipAddress)
        {
            if (!File.Exists(filePath))
            {
                NotificationHelpers.ShowError(IniFileNotFoundMsg, $"File not found: {filePath}", TimeSpan.FromSeconds(5));
                // Прерываем выполнение метода, если файл не найден
                return;
            }

            try
            {
                List<string> lines = [.. File.ReadAllLines(filePath)];
                ProcessIpIniLines(lines, gameSpyIpAddress, ipAddress);
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("INI Write Error", $"Error writing to INI file: {filePath}. {ex.Message}", TimeSpan.FromSeconds(5));
                // Прерываем выполнение метода, если произошла ошибка ввода-вывода
                return;
            }
        }

        private static void ProcessIpIniLines(List<string> lines, string? gameSpyIpAddress, string? ipAddress)
        {
            bool gameSpyShouldBeRemoved = string.IsNullOrWhiteSpace(gameSpyIpAddress) || gameSpyIpAddress == "None";
            bool ipShouldBeRemoved = string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "None";

            var ipConfigurations = new[]
            {
                new IpConfiguration("GameSpyIPAddress =", gameSpyIpAddress, gameSpyShouldBeRemoved),
                new IpConfiguration("IPAddress =", ipAddress, ipShouldBeRemoved)
            };

            var foundFlags = new bool[ipConfigurations.Length];

            ProcessExistingLines(lines, ipConfigurations, foundFlags);
            AddMissingValues(lines, ipConfigurations, foundFlags);
        }

        private static void ProcessExistingLines(List<string> lines, IpConfiguration[] configurations, bool[] foundFlags)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].TrimStart();

                for (int j = 0; j < configurations.Length; j++)
                {
                    if (line.StartsWith(configurations[j].Key, StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = configurations[j].GetValue();
                        foundFlags[j] = true;
                    }
                }
            }
        }

        private static void AddMissingValues(List<string> lines, IpConfiguration[] configurations, bool[] foundFlags)
        {
            for (int j = 0; j < configurations.Length; j++)
            {
                if (!configurations[j].ShouldBeRemoved && !foundFlags[j])
                {
                    lines.Add(configurations[j].GetValue());
                }
            }
        }
    }
}
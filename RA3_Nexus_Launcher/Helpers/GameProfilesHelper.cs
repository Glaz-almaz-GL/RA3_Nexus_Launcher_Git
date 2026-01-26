using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace RA3_Nexus_Launcher.Helpers
{
    public static class GameProfilesHelper
    {
        /// <summary>
        /// Получает список доступных профилей игры из папки профилей.
        /// </summary>
        /// <returns>Список имён доступных игровых профилей.</returns>
        public static List<GameProfile> GetProfilesList()
        {
            // Проверяем, существует ли файл directory.ini
            string directoryIniPath = Path.Combine(PathConstants.RA3ProfilesFolder, "directory.ini");

            if (!File.Exists(directoryIniPath))
            {
                // Логично возвращать пустой список, если файл не найден
                return [];
            }

            // Читаем только первую строку из directory.ini
            string? firstLine = null;
            try
            {
                using FileStream fileStream = new(
                    directoryIniPath,
                    FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                using StreamReader reader = new(fileStream, Encoding.UTF8);
                firstLine = reader.ReadLine();
            }
            catch (IOException ex)
            {
                // Обработка исключения ввода-вывода, например, если файл заблокирован
                Debug.WriteLine($"Ошибка чтения файла directory.ini: {ex.Message}");
                return []; // Возвращаем пустой список в случае ошибки
            }

            if (string.IsNullOrWhiteSpace(firstLine))
            {
                // Если файл пуст или первая строка пуста/нулевая
                return [];
            }

            string original = ParseDirectoryString(firstLine);
            string[] directories = Directory.GetDirectories(PathConstants.RA3ProfilesFolder);
            List<GameProfile> profiles = [];

            foreach (string profilePath in directories)
            {
                string profileName = Path.GetFileNameWithoutExtension(profilePath);

                // Проверяем, существует ли профиль в directory.ini
                if (!string.IsNullOrWhiteSpace(profileName) && original.Contains(profileName))
                {
                    (string? GameSpyIpAddress, string? IpAddress) = GetProfileIpAddresses(Path.Combine(profilePath, "Options.ini"));
                    GameProfile profile = new(profileName, profilePath, IpAddress, GameSpyIpAddress);
                    profiles.Add(profile);
                }
            }
            return profiles;
        }

        /// <summary>
        /// Разбирает строку, закодированную EA (похоже на UTF-8), из файла directory.ini.
        /// Символ '_' обозначает шестнадцатеричное значение следующих двух символов.
        /// </summary>
        /// <param name="input">Закодированная строка.</param>
        /// <returns>Раскодированная строка.</returns>
        private static string ParseDirectoryString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            List<byte> bytes = [];

            int i = 0;
            while (i < input.Length)
            {
                char c = input[i];
                if (c != '_')
                {
                    bytes.Add(Convert.ToByte(c));
                    i++; // Инкремент в обычном случае
                }
                else
                {
                    // Проверяем, есть ли ещё 2 символа после '_'
                    if (i + 1 >= input.Length)
                    {
                        // Недостаточно символов для парсинга шестнадцатеричного числа (только '_')
                        break; // Прерываем цикл, если строка некорректна
                    }
                    if (i + 2 >= input.Length)
                    {
                        // Недостаточно символов для парсинга шестнадцатеричного числа (только '_X')
                        break; // Прерываем цикл, если строка некорректна
                    }

                    string hexPair = input.Substring(i + 1, 2);
                    if (!byte.TryParse(hexPair, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte parsedByte))
                    {
                        throw new FormatException($"Некорректное шестнадцатеричное значение: {hexPair}");
                    }

                    bytes.Add(parsedByte);
                    i += 3; // Пропускаем '_', 'X', 'Y'
                }
            }

            return Encoding.Unicode.GetString([.. bytes]);
        }

        /// <summary>
        /// Обновляет или добавляет значения для GameSpyIPAddress и IPAddress в INI-файле.
        /// </summary>
        /// <param name="filePath">Путь к INI-файлу (например, settings.ini).</param>
        /// <param name="gameSpyIpAddress">Новое значение для GameSpyIPAddress.</param>
        /// <param name="ipAddress">Новое значение для IPAddress.</param>
        public static void UpdateProfileIpAddresses(string filePath, string? gameSpyIpAddress, string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Путь к файлу не может быть null или пустым.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            List<string> lines;
            try
            {
                lines = [.. File.ReadAllLines(filePath)];
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Не удалось прочитать файл {filePath}: {ex.Message}", ex);
            }

            bool gameSpyShouldBeRemoved = string.IsNullOrWhiteSpace(gameSpyIpAddress) || gameSpyIpAddress == "None";
            bool ipShouldBeRemoved = string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "None";

            ProcessIniLines(lines, gameSpyShouldBeRemoved, ipShouldBeRemoved, gameSpyIpAddress, ipAddress);

            try
            {
                File.WriteAllLines(filePath, lines);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Не удалось записать файл {filePath}: {ex.Message}", ex);
            }
        }

        private static void ProcessIniLines(List<string> lines, bool gameSpyShouldBeRemoved, bool ipShouldBeRemoved, string? gameSpyIpAddress, string? ipAddress)
        {
            (bool gameSpyFound, bool ipFound) = ProcessExistingLines(lines, gameSpyShouldBeRemoved, ipShouldBeRemoved, gameSpyIpAddress, ipAddress);

            if (!gameSpyFound)
            {
                string finalGameSpyValue = gameSpyShouldBeRemoved ? "0.0.0.0" : gameSpyIpAddress!;
                lines.Add($"GameSpyIPAddress = {finalGameSpyValue}");
            }
            if (!ipFound)
            {
                string finalIpValue = ipShouldBeRemoved ? "0.0.0.0" : ipAddress!;
                lines.Add($"IPAddress = {finalIpValue}");
            }
        }

        private static (bool gameSpyFound, bool ipFound) ProcessExistingLines(List<string> lines, bool gameSpyShouldBeRemoved, bool ipShouldBeRemoved, string? gameSpyIpAddress, string? ipAddress)
        {
            bool gameSpyFound = false;
            bool ipFound = false;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].TrimStart();

                if (line.StartsWith("GameSpyIPAddress =", StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = gameSpyShouldBeRemoved ? "GameSpyIPAddress = 0.0.0.0" : $"GameSpyIPAddress = {gameSpyIpAddress}";
                    gameSpyFound = true;
                }
                else if (line.StartsWith("IPAddress =", StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = ipShouldBeRemoved ? "IPAddress = 0.0.0.0" : $"IPAddress = {ipAddress}";
                    ipFound = true;
                }
            }

            return (gameSpyFound, ipFound);
        }

        /// <summary>
        /// Получает текущие значения GameSpyIPAddress и IPAddress из INI-файла.
        /// </summary>
        /// <param name="filePath">Путь к INI-файлу (например, settings.ini).</param>
        /// <returns>Кортеж с текущими значениями (GameSpyIPAddress, IPAddress). Если ключи не найдены, возвращаются null.</returns>
        public static (string? GameSpyIpAddress, string? IpAddress) GetProfileIpAddresses(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Путь к файлу не может быть null или пустым.", nameof(filePath));
            }

            // Проверяем, существует ли файл
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            }

            // Читаем все строки из файла
            List<string> lines;
            try
            {
                lines = [.. File.ReadAllLines(filePath)];
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Не удалось прочитать файл {filePath}: {ex.Message}", ex);
            }

            string? gameSpyIpAddress = null;
            string? ipAddress = null;

            // Проходим по строкам и ищем нужные ключи
            foreach (string line in lines)
            {
                string trimmedLine = line.TrimStart();

                // Проверяем, содержит ли строка ключ GameSpyIPAddress
                if (trimmedLine.StartsWith("GameSpyIPAddress =", StringComparison.OrdinalIgnoreCase))
                {
                    // Извлекаем значение после знака равно
                    int equalsIndex = trimmedLine.IndexOf('=');
                    if (equalsIndex >= 0)
                    {
                        gameSpyIpAddress = trimmedLine[(equalsIndex + 1)..].Trim();
                    }
                }
                // Проверяем, содержит ли строка ключ IPAddress
                else if (trimmedLine.StartsWith("IPAddress =", StringComparison.OrdinalIgnoreCase))
                {
                    // Извлекаем значение после знака равно
                    int equalsIndex = trimmedLine.IndexOf('=');
                    if (equalsIndex >= 0)
                    {
                        ipAddress = trimmedLine[(equalsIndex + 1)..].Trim();
                    }
                }
            }

            return (gameSpyIpAddress, ipAddress);
        }

        public static void CheckAndFixSkirmish()
        {
            if (!Directory.Exists(PathConstants.RA3ProfilesFolder))
            {
                return;
            }

            foreach (string profileFolder in Directory.GetDirectories(PathConstants.RA3ProfilesFolder))
            {
                string skirmishIniPath = Path.Combine(profileFolder, "Skirmish.ini");

                if (!File.Exists(skirmishIniPath))
                {
                    continue;
                }

                string[] data = File.ReadAllLines(skirmishIniPath);

                if (data.Length == 0)
                {
                    continue;
                }

                string[] splittedLine = data[0].Split(';');
                if (splittedLine.Length < 2)
                {
                    continue;
                }

                string playerCode = splittedLine[^2].Split(':')[0];

                if (playerCode != "S=X")
                {
                    continue;
                }

                string profileName = Path.GetFileName(profileFolder);
                string fixedPlayerCode = $"S=H{profileName},0,0,TT,-1,7,-1,-1,0,1,-1,:X:X:X:X:X:";
                splittedLine[^2] = fixedPlayerCode;

                StringBuilder sb = new();
                for (int i = 0; i < splittedLine.Length - 1; i++)
                {
                    sb.Append(splittedLine[i]);
                    if (i < splittedLine.Length - 2)
                    {
                        sb.Append(';');
                    }
                }

                data[0] = sb.ToString();
                File.WriteAllLines(skirmishIniPath, data);
            }
        }
    }
}

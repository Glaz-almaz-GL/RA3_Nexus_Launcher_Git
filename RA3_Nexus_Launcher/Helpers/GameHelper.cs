using RA3_Nexus_Launcher.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RA3_Nexus_Launcher.Helpers
{
    public static class GameHelper
    {
        public static string GetGamePath()
        {
            return RegistryHelper.GetRA3Path();
        }

        public static void EnableMaps()
        {
            RegistryHelper.EnableMapSync();
        }

        /// <summary>
        /// Получает список доступных профилей игры из папки профилей.
        /// </summary>
        /// <returns>Список имён доступных игровых профилей.</returns>
        public static List<string> GetProfilesList()
        {
            // Проверяем, существует ли файл directory.ini
            if (!File.Exists(PathConstants.RA3ProfilesFolder + "\\directory.ini"))
            {
                // Логично возвращать пустой список, если файл не найден
                return [];
            }

            // Читаем только первую строку из directory.ini
            string? firstLine = null;
            try
            {
                using var fileStream = new FileStream(
                    PathConstants.RA3ProfilesFolder + "\\directory.ini",
                    FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream, Encoding.UTF8);
                firstLine = reader.ReadLine();
            }
            catch (IOException ex)
            {
                // Обработка исключения ввода-вывода, например, если файл заблокирован
                Console.WriteLine($"Ошибка чтения файла directory.ini: {ex.Message}");
                // В реальном приложении используйте логгер
                return []; // Возвращаем пустой список в случае ошибки
            }

            if (string.IsNullOrEmpty(firstLine))
            {
                // Если файл пуст или первая строка пуста/нулевая
                return [];
            }

            string original = ParseDirectoryString(firstLine);
            string[] directories = Directory.GetDirectories(PathConstants.RA3ProfilesFolder);
            List<string> profiles = [];

            foreach (string profilePath in directories)
            {
                string profileName = Path.GetFileNameWithoutExtension(profilePath);
                // Проверяем, существует ли профиль в directory.ini
                if (!string.IsNullOrEmpty(profileName) && original.Contains(profileName))
                {
                    profiles.Add(profileName);
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
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var bytes = new List<byte>();

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
    }
}
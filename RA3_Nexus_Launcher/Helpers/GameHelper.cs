using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Managers;
using RA3_Nexus_Launcher.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
            RegistryHelper.EnableMaps();
        }

        public static void StartGame(InstalledModInfo? mod = null, string[]? additionalArgs = null)
        {
            var settings = SettingsManager.CurrentSettings;
            string gamePath = settings.GameFolderPath;

            // Проверяем, установлен ли путь к игре
            if (string.IsNullOrEmpty(gamePath))
            {
                throw new InvalidOperationException("Путь к игре Red Alert 3 не установлен в настройках.");
            }

            // Полный путь к исполняемому файлу игры
            string executablePath = Path.Combine(gamePath, "RA3.exe");

            // Проверяем, существует ли исполняемый файл
            if (!File.Exists(executablePath))
            {
                throw new FileNotFoundException($"Исполняемый файл игры не найден: {executablePath}");
            }

            // Собираем аргументы командной строки
            var arguments = new StringBuilder();

            // Добавляем LaunchParameters из настроек (если есть)
            if (settings.LaunchParameters != null)
            {
                foreach (var param in settings.LaunchParameters.Where(param => !string.IsNullOrEmpty(param)))
                {
                    arguments.Append($"\"{param}\" ");
                }
            }

            // *** Логика для мода ***
            bool hasModConfigInSettings = settings.LaunchParameters?.Any(p => p?.Equals("-modconfig", StringComparison.OrdinalIgnoreCase) == true) == true;

            if (mod != null)
            {
                // Если передан мод, добавляем -modconfig с путем к его .skudef файлу
                // Предполагается, что ModPath содержит относительный путь от папки игры или абсолютный путь к .skudef
                // Если ModPath - относительный, нужно составить полный путь:
                string fullModConfigPath = mod.ModPath;
                if (!Path.IsPathRooted(mod.ModPath))
                {
                    fullModConfigPath = Path.Combine(gamePath, mod.ModPath);
                }

                // Проверяем, существует ли файл мода
                if (!File.Exists(fullModConfigPath))
                {
                    throw new FileNotFoundException($"Файл конфигурации мода не найден: {fullModConfigPath}");
                }

                arguments.Append($"-modconfig \"{fullModConfigPath}\" ");
                // Не добавляем -runver, если используется мод
            }
            else
            {
                // Если мод НЕ передан, проверяем, есть ли -modconfig в LaunchParameters
                if (!hasModConfigInSettings)
                {
                    // Если -modconfig НЕ указан ни в LaunchParameters, ни через мод, добавляем -runver
                    arguments.Append($"-runver {settings.RunVersion} ");
                }
                // Если -modconfig указан в LaunchParameters, -runver не добавляем (поведение как в оригинальном коде)
            }

            // Добавляем дополнительные аргументы, переданные в метод (если есть)
            if (additionalArgs != null)
            {
                foreach (var arg in additionalArgs.Where(arg => !string.IsNullOrEmpty(arg)))
                {
                    arguments.Append($"\"{arg}\" ");
                }
            }

            // Убираем последний лишний пробел, если были добавлены аргументы
            if (arguments.Length > 0)
            {
                arguments.Length--; // Удаляем последний символ (пробел)
            }

            // Запускаем процесс
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments.ToString(),
                    WorkingDirectory = gamePath, // Устанавливаем рабочую директорию в папку игры
                    UseShellExecute = false // Рекомендуется для передачи аргументов
                };

                using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Не удалось запустить процесс игры.");
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                throw new InvalidOperationException($"Ошибка при запуске игры: {ex.Message}", ex);
            }
        }
    }
}
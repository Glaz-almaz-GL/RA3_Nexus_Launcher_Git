using RA3_Nexus_Launcher.Managers;
using RA3_Nexus_Launcher.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace RA3_Nexus_Launcher.Helpers
{
    public static class GameHelper
    {
        public static string GetGameFolder()
        {
            return GameRegistryHelper.GetRA3Path();
        }

        public static void EnableMaps()
        {
            GameRegistryHelper.EnableMaps();
        }

        public static void StartGame(InstalledModInfo? mod = null, string[]? additionalArgs = null)
        {
            ValidateGamePath();
            GameProfilesHelper.CheckAndFixSkirmish();

            Settings settings = SettingsManager.CurrentSettings;
            string executablePath = settings.GamePath;

            string arguments = BuildArguments(settings, mod, additionalArgs);

            // Запускаем процесс
            try
            {
                ProcessStartInfo startInfo = new()
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    WorkingDirectory = settings.GameFolderPath,
                    UseShellExecute = false
                };

                using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Не удалось запустить процесс игры.");
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                throw new InvalidOperationException($"Ошибка при запуске игры: {ex.Message}", ex);
            }
        }

        private static void ValidateGamePath()
        {
            Settings settings = SettingsManager.CurrentSettings;
            string gamePath = settings.GameFolderPath;

            if (string.IsNullOrWhiteSpace(gamePath))
            {
                throw new InvalidOperationException("Путь к игре Red Alert 3 не установлен в настройках.");
            }

            string executablePath = settings.GamePath;

            if (!File.Exists(executablePath))
            {
                throw new FileNotFoundException($"Исполняемый файл игры не найден: {executablePath}");
            }
        }

        private static string BuildArguments(Settings settings, InstalledModInfo? mod, string[]? additionalArgs)
        {
            StringBuilder arguments = new();

            AddLaunchParameters(arguments, settings.LaunchParameters);

            AddModOrVersionArguments(arguments, settings, mod);

            AddAdditionalArguments(arguments, additionalArgs);

            // Убираем последний лишний пробел, если были добавлены аргументы
            if (arguments.Length > 0)
            {
                arguments.Length--;
            }

            return arguments.ToString();
        }

        private static void AddLaunchParameters(StringBuilder arguments, string[]? launchParameters)
        {
            if (launchParameters != null)
            {
                foreach (string? param in launchParameters.Where(param => !string.IsNullOrWhiteSpace(param)))
                {
                    arguments.Append($"\"{param}\" ");
                }
            }
        }

        private static void AddModOrVersionArguments(StringBuilder arguments, Settings settings, InstalledModInfo? mod)
        {
            bool hasModConfigInSettings = settings.LaunchParameters?.Any(p => p?.Equals("-modconfig", StringComparison.OrdinalIgnoreCase) == true) == true;

            if (mod != null)
            {
                if (!File.Exists(mod.ModPath))
                {
                    throw new FileNotFoundException($"Файл конфигурации мода не найден: {mod.ModPath}");
                }

                arguments.Append($"-modconfig \"{mod.ModPath}\" ");
            }
            else
            {
                if (!hasModConfigInSettings)
                {
                    arguments.Append($"-runver {settings.RunVersion} ");
                }
            }
        }

        private static void AddAdditionalArguments(StringBuilder arguments, string[]? additionalArgs)
        {
            if (additionalArgs != null)
            {
                foreach (string? arg in additionalArgs.Where(arg => !string.IsNullOrWhiteSpace(arg)))
                {
                    arguments.Append($"\"{arg}\" ");
                }
            }
        }
    }
}
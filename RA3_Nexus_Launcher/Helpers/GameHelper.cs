using RA3_Nexus_Launcher.Managers;
using RA3_Nexus_Launcher.Models;
using RA3_Nexus_Launcher.Helpers; // Добавьте этот using для NotificationHelpers
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
        public static string GetGameFolder() => GameRegistryHelper.GetRA3Path();

        public static string? GetBattleNetPath() => GameRegistryHelper.GetRA3BattleNetPath();

        public static void EnableMaps() => GameRegistryHelper.EnableMaps();

        public static void StartGame(InstalledModInfo? mod = null, string[]? additionalArgs = null)
        {
            if (!ValidateGamePath()) return; // Проверяем путь и выходим, если ошибка
            SkirmishFixer.CheckAndFixSkirmish();

            LauncherSettings settings = SettingsManager.CurrentSettings;
            string executablePath = settings.GamePath;

            string arguments;
            try
            {
                arguments = BuildArguments(settings, mod, additionalArgs);
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Argument Build Error", $"Failed to build launch arguments: {ex.Message}", TimeSpan.FromSeconds(5));
                return; // Прерываем запуск
            }

            // Запускаем процесс
            try
            {
                ProcessStartInfo startInfo = new()
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    WorkingDirectory = settings.GameFolder,
                    UseShellExecute = false
                };

                using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start the game process.");
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                NotificationHelpers.ShowError("Game Start Error", $"Error starting the game: {ex.Message}", TimeSpan.FromSeconds(5));
                // Не выбрасываем исключение, просто показываем уведомление
            }
        }

        private static bool ValidateGamePath() // Изменили на bool
        {
            LauncherSettings settings = SettingsManager.CurrentSettings;
            string gamePath = settings.GameFolder;

            if (string.IsNullOrWhiteSpace(gamePath))
            {
                NotificationHelpers.ShowError("Game Path Not Set", "The path to the Red Alert 3 game is not set in the settings.", TimeSpan.FromSeconds(5));
                return false; // Возвращаем false в случае ошибки
            }

            string executablePath = settings.GamePath;

            if (!File.Exists(executablePath))
            {
                NotificationHelpers.ShowError("Game Executable Not Found", $"Game executable file not found: {executablePath}", TimeSpan.FromSeconds(5));
                return false; // Возвращаем false в случае ошибки
            }

            return true; // Возвращаем true, если всё ок
        }

        private static string BuildArguments(LauncherSettings settings, InstalledModInfo? mod, string[]? additionalArgs)
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

        private static void AddModOrVersionArguments(StringBuilder arguments, LauncherSettings settings, InstalledModInfo? mod)
        {
            bool hasModConfigInSettings = settings.LaunchParameters?.Any(p => p?.Equals("-modconfig", StringComparison.OrdinalIgnoreCase) == true) == true;

            if (mod != null)
            {
                if (!File.Exists(mod.ModPath))
                {
                    NotificationHelpers.ShowError("Mod File Not Found", $"Mod configuration file not found: {mod.ModPath}", TimeSpan.FromSeconds(5));
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
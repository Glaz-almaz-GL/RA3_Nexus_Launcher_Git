using RA3_Nexus_Launcher.Constants;
using System;
using System.Diagnostics;
using System.IO;

namespace RA3_Nexus_Launcher.Helpers.Patches
{
    public static class RegistryFixHelper
    {
        public static void FixRegistry()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                ApplyRegFile(PathConstants.RA3RegistryFix64);
            }
            else
            {
                ApplyRegFile(PathConstants.RA3RegistryFix32);
            }
        }

        private static void ApplyRegFile(string? regFilePath)
        {
            if (!File.Exists(regFilePath))
            {
                Debug.WriteLine($"Файл реестра не найден: {regFilePath}");
                return;
            }

            try
            {
                // Используем ProcessStartInfo для безопасного запуска
                ProcessStartInfo startInfo = new()
                {
                    FileName = "regedit.exe",
                    Arguments = $"/s \"{regFilePath}\"", // /s - подавляет запросы подтверждения
                    UseShellExecute = false, // Позволяет перенаправить потоки, но для regedit необязательно
                    CreateNoWindow = true, // Скрывает окно regedit
                    Verb = "runas" // Запускает с правами администратора (вызовет UAC)
                };

                using Process? process = Process.Start(startInfo);

                if (process != null)
                {
                    process.WaitForExit(); // Ждем завершения выполнения regedit

                    if (process.ExitCode == 0)
                    {
                        Debug.WriteLine("Файл реестра успешно применен.");
                    }
                    else
                    {
                        throw new InvalidOperationException($"Ошибка при применении файла реестра. Код выхода: {process.ExitCode}");
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                ProcessStartInfo process = new()
                {
                    FileName = Environment.ProcessPath!,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(process);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}

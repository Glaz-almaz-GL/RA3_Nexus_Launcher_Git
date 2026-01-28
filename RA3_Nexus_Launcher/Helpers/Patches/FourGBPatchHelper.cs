using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Managers;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace RA3_Nexus_Launcher.Helpers.Patches
{
    public static class FourGBPatchHelper
    {
        // Код ошибки для "The requested operation requires elevation"
        private const int ERROR_ELEVATION_REQUIRED = 740;

        public static void Install4GBPatch()
        {
            try
            {
                ProcessStartInfo fourGbPatchInfo = new()
                {
                    FileName = PathConstants.RA3FourGBPatch,
                    Arguments = "/S",
                    CreateNoWindow = true, // Скрывает окно
                    Verb = "runas" // Запускает с правами администратора (вызовет UAC)
                };

                using Process? process = Process.Start(fourGbPatchInfo);

                if (process != null)
                {
                    process.WaitForExit(); // Ждем завершения выполнения regedit

                    if (process.ExitCode == 0)
                    {
                        Debug.WriteLine("4GB Патч применён успешно применен.");
                    }
                    else
                    {
                        throw new InvalidOperationException($"Ошибка при применении 4GB патча. Код выхода: {process.ExitCode}");
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                HandleWin32Exception(ex);
            }
            catch (Exception ex)
            {
                HandleGeneralException(ex);
            }
        }

        private static void HandleWin32Exception(System.ComponentModel.Win32Exception ex)
        {
            if (ex.NativeErrorCode == ERROR_ELEVATION_REQUIRED)
            {
                GamePatchesManager.RestartWithAdministratorPrivileges();
                return;
            }

            // Возникает, если процесс не найден или пользователь отказался от UAC
            throw new InvalidOperationException($"Ошибка запуска 4GB-Patch: {ex.Message}");
        }

        private static void HandleGeneralException(Exception ex)
        {
            // Проверяем внутреннее исключение на наличие Win32Exception с нужным кодом
            Win32Exception? innerEx = ex.InnerException as System.ComponentModel.Win32Exception;
            if (innerEx?.NativeErrorCode == ERROR_ELEVATION_REQUIRED)
            {
                GamePatchesManager.RestartWithAdministratorPrivileges();
                return;
            }

            throw new InvalidOperationException($"Произошла ошибка: {ex.Message}");
        }
    }
}
using Microsoft.Win32;
using RA3_Nexus_Launcher.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Text;

namespace RA3_Nexus_Launcher.Helpers.Patches
{
    public static class CDKeyHelper
    {
        public static void ApplyCDKey(string cdKey)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Electronic Arts\Electronic Arts\Red Alert 3\ergc", writable: true);
                    if (key != null)
                    {
                        // Изменяем значение по умолчанию (Default Value) - это значение с именем "(Default)" в редакторе реестра
                        key.SetValue("", cdKey);

                        // Изменяем значение с именем "(Default)" (именованное значение)
                        key.SetValue("Default", cdKey);
                    }
                    else
                    {
                        // Ключ не найден
                        throw new KeyNotFoundException($"Ключ реестра {cdKey} не найден.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Операции с реестром Windows недоступны на этой платформе.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Сообщаем пользователю, что требуется запуск от имени администратора
                Debug.WriteLine("Для выполнения этой операции необходимы права администратора.");

                try
                {
                    GamePatchesManager.RestartWithAdministratorPrivileges();
                }
                catch (Exception ex)
                {
                    // Если не удалось запустить от администратора (например, пользователь отказался)
                    Debug.WriteLine($"Не удалось запустить приложение от имени администратора: {ex.Message}");
                    throw new InvalidOperationException("Для выполнения операции необходимы права администратора.", ex);
                }

                // Закрываем текущий процесс
                Environment.Exit(0);
            }
            catch (SecurityException)
            {
                // Аналогично обрабатываем SecurityException
                Debug.WriteLine("Для выполнения этой операции необходимы права администратора.");

                // Перезапускаем приложение от имени администратора
                try
                {
                    GamePatchesManager.RestartWithAdministratorPrivileges();
                }
                catch (Exception ex)
                {
                    // Если не удалось запустить от администратора (например, пользователь отказался)
                    Debug.WriteLine($"Не удалось запустить приложение от имени администратора: {ex.Message}");
                    throw new InvalidOperationException("Для выполнения операции необходимы права администратора.", ex);
                }

                Environment.Exit(0);
            }
            catch (Exception ex) // Общий блок для других ошибок
            {
                throw new InvalidOperationException($"Произошла ошибка применения CD ключа: {ex.Message}", ex);
            }
        }

        public static string GenerateCDKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new();
            StringBuilder result = new();

            for (int i = 0; i < 20; i++) // 20 символов без разделителей
            {
                result.Append(chars[random.Next(chars.Length)]);
                if ((i + 1) % 4 == 0 && i != 19) // Добавляем "-" после каждых 4 символов
                {
                    result.Append('-');
                }
            }

            return result.ToString();
        }
    }
}

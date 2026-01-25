using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;

namespace RA3_Nexus_Launcher.Helpers
{
    /// <summary>
    /// Предоставляет вспомогательные методы для работы с записями реестра Red Alert 3.
    /// </summary>
#pragma warning disable CA1416 // Проверка совместимости платформы
    public static class RegistryHelper
    {
        // Константы для уменьшения дублирования строк
        private const string RA3_REGISTRY_PATH = @"Software\Electronic Arts\Electronic Arts\Red Alert 3";
        private const string INSTALL_DIR_VALUE_NAME = "Install Dir";
        private const string USE_LOCAL_USER_MAP_VALUE_NAME = "UseLocalUserMap";
        private const string LANGUAGE_VALUE_NAME = "Language";

        /// <summary>
        /// Возможные состояния реестра RA3.
        /// </summary>
        public enum RegistryStatus
        {
            Correct,        // Все необходимые значения присутствуют и верны
            NotExist,       // Ключ реестра RA3 не существует
            MissingPath,    // Отсутствует Install Dir
            MissingMapSync, // Отсутствует UseLocalUserMap или его значение не 0
            MissingLanguage // Отсутствует Language в пользовательском ключе
        }

        /// <summary>
        /// Получает текущее состояние реестра RA3.
        /// </summary>
        public static RegistryStatus Status => IsRegistryValid();

        /// <summary>
        /// Проверяет, все ли необходимые записи в реестре RA3 присутствуют и корректны.
        /// Использует представление RegistryView.Registry32 для доступа к WOW6432Node.
        /// </summary>
        /// <returns>Статус проверки.</returns>
        private static RegistryStatus IsRegistryValid()
        {
            Debug.WriteLine("Начинается проверка реестра RA3 (представление 32-бит)...");

            // Используем RegistryView.Registry32 для доступа к WOW6432Node
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using var key32 = baseKey.OpenSubKey(RA3_REGISTRY_PATH, writable: false);

            if (key32 == null)
            {
                Debug.WriteLine($"Ключ реестра HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH} не найден.");
                return RegistryStatus.NotExist;
            }
            Debug.WriteLine($"Ключ реестра HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH} найден.");

            var installDirValue = key32.GetValue(INSTALL_DIR_VALUE_NAME);
            if (installDirValue == null)
            {
                Debug.WriteLine($"Значение '{INSTALL_DIR_VALUE_NAME}' в ключе HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH} отсутствует.");
                return RegistryStatus.MissingPath;
            }
            Debug.WriteLine($"Значение '{INSTALL_DIR_VALUE_NAME}' в ключе HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH} найдено: {installDirValue}");

            var mapSyncValue = key32.GetValue(USE_LOCAL_USER_MAP_VALUE_NAME);
            if (mapSyncValue == null)
            {
                Debug.WriteLine($"Значение '{USE_LOCAL_USER_MAP_VALUE_NAME}' в ключе HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH} отсутствует.");
                return RegistryStatus.MissingMapSync;
            }
            int mapSyncIntValue = Convert.ToInt32(mapSyncValue);
            if (mapSyncIntValue != 0)
            {
                Debug.WriteLine($"Значение '{USE_LOCAL_USER_MAP_VALUE_NAME}' в ключе HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH} равно {mapSyncIntValue}, ожидалось 0.");
                return RegistryStatus.MissingMapSync;
            }
            Debug.WriteLine($"Значение '{USE_LOCAL_USER_MAP_VALUE_NAME}' в ключе HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH} корректно (равно 0).");

            // Для пользовательских ключей RegistryView обычно не требуется, используем Registry.CurrentUser
            using var userBaseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default); // Default безопасен для CurrentUser
            using var userKey = userBaseKey.OpenSubKey(RA3_REGISTRY_PATH, writable: false);

            if (userKey == null)
            {
                Debug.WriteLine($"Ключ реестра HKEY_CURRENT_USER\\{RA3_REGISTRY_PATH} не найден.");
                return RegistryStatus.MissingLanguage;
            }
            Debug.WriteLine($"Ключ реестра HKEY_CURRENT_USER\\{RA3_REGISTRY_PATH} найден.");

            var languageValue = userKey.GetValue(LANGUAGE_VALUE_NAME);
            if (languageValue == null)
            {
                Debug.WriteLine($"Значение '{LANGUAGE_VALUE_NAME}' в ключе HKEY_CURRENT_USER\\{RA3_REGISTRY_PATH} отсутствует.");
                return RegistryStatus.MissingLanguage;
            }
            Debug.WriteLine($"Значение '{LANGUAGE_VALUE_NAME}' в ключе HKEY_CURRENT_USER\\{RA3_REGISTRY_PATH} найдено: {languageValue}");

            Debug.WriteLine("Проверка реестра RA3 завершена успешно. Статус: Correct.");
            return RegistryStatus.Correct;
        }

        /// <summary>
        /// Получает путь к установленной игре RA3 из реестра.
        /// Если статус реестра MissingMapSync, пытается включить синхронизацию и проверить снова.
        /// </summary>
        /// <returns>Путь к папке установки или пустая строка, если путь не найден или не удалось исправить статус.</returns>
        public static string GetRA3Path()
        {
            var status = IsRegistryValid();
            Debug.WriteLine($"Первоначальный статус реестра: {status}");

            // Проверяем, нуждается ли статус в исправлении перед попыткой получения пути
            if (status == RegistryStatus.MissingMapSync)
            {
                Debug.WriteLine("Обнаружен статус MissingMapSync. Попытка включить синхронизацию карт...");
                try
                {
                    EnableMaps(); // Пытаемся исправить статус
                    Debug.WriteLine("Синхронизация карт включена. Повторная проверка статуса...");

                    // Повторно проверяем статус после EnableMapSync
                    status = IsRegistryValid();
                    Debug.WriteLine($"Статус реестра после EnableMapSync: {status}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при попытке включить синхронизацию карт: {ex.Message}");
                    // Если EnableMapSync завершился с ошибкой, статус остаётся прежним (MissingMapSync),
                    // и мы не пытаемся получить путь.
                    return string.Empty;
                }
            }

            // Теперь, если статус Correct, пытаемся получить путь
            if (status == RegistryStatus.Correct)
            {
                // Повторно открываем ключ, так как EnableMapSync мог его изменить
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                using var key = baseKey.OpenSubKey(RA3_REGISTRY_PATH, writable: false);
                var pathValue = key?.GetValue(INSTALL_DIR_VALUE_NAME) as string;
                Debug.WriteLine($"Получен путь из реестра: {pathValue ?? "(null или пустой)"}");
                return pathValue ?? string.Empty;
            }
            else
            {
                Debug.WriteLine($"Путь к игре не получен, так как статус реестра: {status}");
                return string.Empty;
            }
        }

        public static void SetRA3Path(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Путь к игре не может быть null или пустым.", nameof(path));
            }

            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using var subKey = baseKey.OpenSubKey(RA3_REGISTRY_PATH, writable: true);

            if (subKey == null)
            {
                // Ключ не существует, создаем его
                using var newSubKey = baseKey.CreateSubKey(RA3_REGISTRY_PATH, writable: true) ?? throw new InvalidOperationException($"Не удалось создать ключ реестра '{RA3_REGISTRY_PATH}' в HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node.");
                newSubKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
                Debug.WriteLine($"Значение '{INSTALL_DIR_VALUE_NAME}' установлено в '{path}' в HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH}");
            }
            else
            {
                // Ключ существует, просто устанавливаем значение
                subKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
                Debug.WriteLine($"Значение '{INSTALL_DIR_VALUE_NAME}' обновлено на '{path}' в HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH}");
            }
        }

        public static bool SetLanguage(string gamePath, string language)
        {
            if (string.IsNullOrEmpty(gamePath) || string.IsNullOrEmpty(language))
            {
                return false; // Некорректные входные данные
            }

            try
            {
                // Проверяем, содержит ли папка игры хотя бы один .skudef файл
                var isPathValid = Directory.EnumerateFiles(gamePath, "*.skudef").Any();
                if (!isPathValid)
                {
                    return false;
                }

                // Внутренние функции для формирования путей
                string GetCsfPath(string lang) => Path.Combine(gamePath, "Launcher", $"{lang}.csf");
                string GetSkudefPath(string lang, string version) => Path.Combine(gamePath, $"RA3_{lang}_{version}.skudef");

                // Проверяем наличие файлов для указанного языка
                bool languageFilesExist = File.Exists(GetSkudefPath(language, "1.12")) && File.Exists(GetCsfPath(language));

                // Если файлы указанного языка не найдены, пробуем английский как fallback
                string? finalLanguage;
                if (languageFilesExist)
                {
                    finalLanguage = language;
                }
                else if (File.Exists(GetSkudefPath("english", "1.12")) && File.Exists(GetCsfPath("english")))
                {
                    finalLanguage = "english";
                }
                else
                {
                    finalLanguage = null;
                }

                if (finalLanguage == null)
                {
                    // Ни указанный язык, ни английский не найдены
                    return false;
                }

                // Устанавливаем язык в реестре (CurrentUser)
                using var baseUserKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
                using var userSubKey = baseUserKey.OpenSubKey(RA3_REGISTRY_PATH, writable: true);

                if (userSubKey == null)
                {
                    // Ключ не существует, создаем его
                    using var newUserSubKey = baseUserKey.CreateSubKey(RA3_REGISTRY_PATH, writable: true);
                    if (newUserSubKey == null)
                    {
                        // Не удалось создать ключ
                        Debug.WriteLine($"Не удалось создать ключ реестра '{RA3_REGISTRY_PATH}' в HKEY_CURRENT_USER для установки языка.");
                        return false;
                    }
                    newUserSubKey.SetValue(LANGUAGE_VALUE_NAME, finalLanguage, RegistryValueKind.String);
                    Debug.WriteLine($"Значение '{LANGUAGE_VALUE_NAME}' установлено в '{finalLanguage}' в HKEY_CURRENT_USER\\{RA3_REGISTRY_PATH}");
                }
                else
                {
                    // Ключ существует, устанавливаем значение
                    userSubKey.SetValue(LANGUAGE_VALUE_NAME, finalLanguage, RegistryValueKind.String);
                    Debug.WriteLine($"Значение '{LANGUAGE_VALUE_NAME}' обновлено на '{finalLanguage}' в HKEY_CURRENT_USER\\{RA3_REGISTRY_PATH}");
                }

                return true; // Успешно установлен язык (возможно, fallback)
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException or IOException)
            {
                Debug.WriteLine($"Ошибка при установке языка в реестре: {ex.Message}");
                return false; // Возвращаем false в случае ошибки доступа или ввода-вывода
            }
        }

        public static void EnableMaps()
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using var subKey = baseKey.OpenSubKey(RA3_REGISTRY_PATH, writable: true);

            if (subKey == null)
            {
                // Ключ не существует, создаем его
                using var newSubKey = baseKey.CreateSubKey(RA3_REGISTRY_PATH, writable: true) ?? throw new InvalidOperationException($"Не удалось создать ключ реестра '{RA3_REGISTRY_PATH}' в HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node.");
                newSubKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
                Debug.WriteLine($"Значение '{USE_LOCAL_USER_MAP_VALUE_NAME}' установлено в 0 в HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH}");
            }
            else
            {
                // Ключ существует, устанавливаем значение
                subKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
                Debug.WriteLine($"Значение '{USE_LOCAL_USER_MAP_VALUE_NAME}' обновлено на 0 в HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH}");
            }
        }

        public static void ResetRegistry(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Путь к игре не может быть null или пустым.", nameof(path));
            }

            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using var subKey = baseKey.OpenSubKey(RA3_REGISTRY_PATH, writable: true);

            if (subKey == null)
            {
                // Ключ не существует, создаем его
                using var newSubKey = baseKey.CreateSubKey(RA3_REGISTRY_PATH, writable: true) ?? throw new InvalidOperationException($"Не удалось создать ключ реестра '{RA3_REGISTRY_PATH}' в HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node.");
                newSubKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
                newSubKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
                Debug.WriteLine($"Ключ HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH} создан с параметрами Install Dir='{path}', UseLocalUserMap=0");
            }
            else
            {
                // Ключ существует, обновляем значения
                subKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
                subKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
                Debug.WriteLine($"Ключ HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RA3_REGISTRY_PATH} обновлён: Install Dir='{path}', UseLocalUserMap=0");
            }
        }
    }
#pragma warning restore CA1416
}
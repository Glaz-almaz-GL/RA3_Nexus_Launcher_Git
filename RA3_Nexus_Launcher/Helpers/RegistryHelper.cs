using Microsoft.Win32;
using System;
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
        /// </summary>
        /// <returns>Статус проверки.</returns>
        private static RegistryStatus IsRegistryValid()
        {
            using var key32 = Registry.LocalMachine.OpenSubKey(RA3_REGISTRY_PATH, writable: false);
            if (key32 == null)
            {
                return RegistryStatus.NotExist;
            }

            if (key32.GetValue(INSTALL_DIR_VALUE_NAME) == null)
            {
                return RegistryStatus.MissingPath;
            }

            var mapSyncValue = key32.GetValue(USE_LOCAL_USER_MAP_VALUE_NAME);
            if (mapSyncValue == null || Convert.ToInt32(mapSyncValue) != 0)
            {
                return RegistryStatus.MissingMapSync;
            }

            // Исправлено: использовать Registry.CurrentUser вместо view32 для CurrentUser
            using var userKey = Registry.CurrentUser.OpenSubKey(RA3_REGISTRY_PATH, writable: false);
            if (userKey == null || userKey.GetValue(LANGUAGE_VALUE_NAME) == null)
            {
                return RegistryStatus.MissingLanguage;
            }

            return RegistryStatus.Correct;
        }

        /// <summary>
        /// Получает путь к установленной игре RA3 из реестра.
        /// </summary>
        /// <returns>Путь к папке установки или пустая строка, если путь не найден.</returns>
        public static string GetRA3Path()
        {
            using var key = Registry.LocalMachine.OpenSubKey(RA3_REGISTRY_PATH, writable: false);
            return key?.GetValue(INSTALL_DIR_VALUE_NAME) as string ?? string.Empty;
        }

        /// <summary>
        /// Устанавливает путь к установленной игре RA3 в реестре.
        /// Создает ключ, если он не существует.
        /// </summary>
        /// <param name="path">Путь к папке установки игры.</param>
        /// <exception cref="UnauthorizedAccessException">Если нет прав для записи в реестр.</exception>
        public static void SetRA3Path(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Путь к игре не может быть null или пустым.", nameof(path));
            }

            using var baseKey = Registry.LocalMachine;
            using var subKey = baseKey.OpenSubKey(RA3_REGISTRY_PATH, writable: true);

            if (subKey == null)
            {
                // Ключ не существует, создаем его
                using var newSubKey = baseKey.CreateSubKey(RA3_REGISTRY_PATH, writable: true) ?? throw new InvalidOperationException($"Не удалось создать ключ реестра '{RA3_REGISTRY_PATH}' в LocalMachine.");
                newSubKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
            }
            else
            {
                // Ключ существует, просто устанавливаем значение
                subKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Устанавливает язык игры в реестре, проверяя наличие соответствующих файлов.
        /// </summary>
        /// <param name="gamePath">Путь к папке установки игры.</param>
        /// <param name="language">Код языка (например, 'english', 'russian').</param>
        /// <returns>True, если язык успешно установлен; иначе false.</returns>
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

                if (string.IsNullOrWhiteSpace(finalLanguage))
                {
                    // Ни указанный язык, ни английский не найдены
                    return false;
                }

                // Устанавливаем язык в реестре
                using var baseKey = Registry.CurrentUser;
                using var subKey = baseKey.OpenSubKey(RA3_REGISTRY_PATH, writable: true);

                if (subKey == null)
                {
                    // Ключ не существует, создаем его
                    using var newSubKey = baseKey.CreateSubKey(RA3_REGISTRY_PATH, writable: true);
                    if (newSubKey == null)
                    {
                        // Не удалось создать ключ
                        return false;
                    }
                    newSubKey.SetValue(LANGUAGE_VALUE_NAME, finalLanguage, RegistryValueKind.String);
                }
                else
                {
                    // Ключ существует, устанавливаем значение
                    subKey.SetValue(LANGUAGE_VALUE_NAME, finalLanguage, RegistryValueKind.String);
                }

                return true; // Успешно установлен язык (возможно, fallback)
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException or IOException)
            {
                return false; // Возвращаем false в случае ошибки доступа или ввода-вывода
            }
        }

        /// <summary>
        /// Включает синхронизацию карт (устанавливает UseLocalUserMap в 0).
        /// Создает ключ, если он не существует.
        /// </summary>
        public static void EnableMapSync()
        {
            using var baseKey = Registry.LocalMachine;
            using var subKey = baseKey.OpenSubKey(RA3_REGISTRY_PATH, writable: true);

            if (subKey == null)
            {
                // Ключ не существует, создаем его
                using var newSubKey = baseKey.CreateSubKey(RA3_REGISTRY_PATH, writable: true) ?? throw new InvalidOperationException($"Не удалось создать ключ реестра '{RA3_REGISTRY_PATH}' в LocalMachine.");
                newSubKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
            }
            else
            {
                // Ключ существует, устанавливаем значение
                subKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Сбрасывает (устанавливает или обновляет) пути и настройки синхронизации в реестре.
        /// </summary>
        /// <param name="path">Путь к папке установки игры.</param>
        public static void ResetRegistry(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Путь к игре не может быть null или пустым.", nameof(path));
            }

            using var baseKey = Registry.LocalMachine;
            using var subKey = baseKey.OpenSubKey(RA3_REGISTRY_PATH, writable: true);

            if (subKey == null)
            {
                // Ключ не существует, создаем его
                using var newSubKey = baseKey.CreateSubKey(RA3_REGISTRY_PATH, writable: true) ?? throw new InvalidOperationException($"Не удалось создать ключ реестра '{RA3_REGISTRY_PATH}' в LocalMachine.");
                newSubKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
                newSubKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
            }
            else
            {
                // Ключ существует, обновляем значения
                subKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
                subKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
            }
        }
    }
#pragma warning restore CA1416
}
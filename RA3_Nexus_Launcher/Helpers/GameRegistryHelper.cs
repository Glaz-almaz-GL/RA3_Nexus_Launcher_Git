using Microsoft.Win32;
using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Helpers; // Ensure NotificationHelpers is accessible
using RA3_Nexus_Launcher.Managers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;

namespace RA3_Nexus_Launcher.Helpers
{
    /// <summary>
    /// Provides helper methods for interacting with Red Alert 3 registry entries.
    /// </summary>
#pragma warning disable CA1416 // Validate platform compatibility
    public static class GameRegistryHelper
    {
        // Constants to reduce string duplication
        private const string INSTALL_DIR_VALUE_NAME = "Install Dir";
        private const string USE_LOCAL_USER_MAP_VALUE_NAME = "UseLocalUserMap";
        private const string LANGUAGE_VALUE_NAME = "Language";
        private const string REGISTRY_PATH = $"HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RegistryConstants.RA3GamePath}";
        private const string DefaultLanguageName = "russian";
        private const string RegistryErrorTitle = "Registry Error";
        private const string GamePathIsNullOrEmptyMsg = "The game path cannot be null or empty.";

        /// <summary>
        /// Possible states of the RA3 registry.
        /// </summary>
        public enum RegistryStatus
        {
            Correct,        // All required values are present and correct
            NotExist,       // The RA3 registry key does not exist
            MissingPath,    // Install Dir is missing
            MissingMapSync, // UseLocalUserMap is missing or its value is not 0
            MissingLanguage // Language is missing in the user key
        }

        /// <summary>
        /// Checks if all required RA3 registry entries are present and correct.
        /// Uses RegistryView.Registry32 to access WOW6432Node.
        /// </summary>
        /// <returns>The check status.</returns>
        private static RegistryStatus IsRegistryValid()
        {
            Debug.WriteLine("Starting check of RA3 registry (32-bit representation)...");

            // Use RegistryView.Registry32 to access WOW6432Node
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using RegistryKey? key32 = baseKey.OpenSubKey(RegistryConstants.RA3GamePath, writable: false);

            if (key32 == null)
            {
                Debug.WriteLine($"Registry key {REGISTRY_PATH} not found.");
                return RegistryStatus.NotExist;
            }
            Debug.WriteLine($"Registry key {REGISTRY_PATH} found.");

            object? installDirValue = key32.GetValue(INSTALL_DIR_VALUE_NAME);
            if (installDirValue == null)
            {
                Debug.WriteLine($"The value '{INSTALL_DIR_VALUE_NAME}' is missing from the {REGISTRY_PATH} key.");
                return RegistryStatus.MissingPath;
            }
            Debug.WriteLine($"Value '{INSTALL_DIR_VALUE_NAME}' in key {REGISTRY_PATH} found: {installDirValue}");

            object? mapSyncValue = key32.GetValue(USE_LOCAL_USER_MAP_VALUE_NAME);
            if (mapSyncValue == null)
            {
                Debug.WriteLine($"The value '{USE_LOCAL_USER_MAP_VALUE_NAME}' is missing from the {REGISTRY_PATH} key.");
                return RegistryStatus.MissingMapSync;
            }
            int mapSyncIntValue = Convert.ToInt32(mapSyncValue);
            if (mapSyncIntValue != 0)
            {
                Debug.WriteLine($"The value '{USE_LOCAL_USER_MAP_VALUE_NAME}' in key {REGISTRY_PATH} is {mapSyncIntValue}, expected 0.");
                return RegistryStatus.MissingMapSync;
            }
            Debug.WriteLine($"The value '{USE_LOCAL_USER_MAP_VALUE_NAME}' in the {REGISTRY_PATH} key is correct (equal to 0).");

            // For user keys, RegistryView is usually not needed, use Registry.CurrentUser
            using RegistryKey userBaseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default); // Default is safe for CurrentUser
            using RegistryKey? userKey = userBaseKey.OpenSubKey(RegistryConstants.RA3GamePath, writable: false);

            if (userKey == null)
            {
                Debug.WriteLine($"The registry key HKEY_CURRENT_USER\\{RegistryConstants.RA3GamePath} was not found.");
                return RegistryStatus.MissingLanguage;
            }
            Debug.WriteLine($"Registry key HKEY_CURRENT_USER\\{RegistryConstants.RA3GamePath} found.");

            object? languageValue = userKey.GetValue(LANGUAGE_VALUE_NAME);
            if (languageValue == null)
            {
                Debug.WriteLine($"The value '{LANGUAGE_VALUE_NAME}' is missing in the HKEY_CURRENT_USER\\{RegistryConstants.RA3GamePath} key.");
                return RegistryStatus.MissingLanguage;
            }
            Debug.WriteLine($"The value '{LANGUAGE_VALUE_NAME}' in the key HKEY_CURRENT_USER\\{RegistryConstants.RA3GamePath} was found: {languageValue}");

            Debug.WriteLine("The RA3 registry check completed successfully. Status: Correct.");
            return RegistryStatus.Correct;
        }

        /// <summary>
        /// Gets the path to the installed RA3 game from the registry.
        /// If the registry status is MissingMapSync, attempts to enable sync and check again.
        /// </summary>
        /// <returns>The installation folder path or an empty string if the path is not found or status cannot be fixed.</returns>
        public static string GetRA3Path()
        {
            RegistryStatus status = IsRegistryValid();
            Debug.WriteLine($"Initial registry status: {status}");

            // Check if the status needs fixing before attempting to get the path
            if (status == RegistryStatus.MissingMapSync)
            {
                Debug.WriteLine("MissingMapSync status detected. Attempting to enable map sync...");
                try
                {
                    EnableMaps(); // Try to fix the status
                    Debug.WriteLine("Map sync enabled. Re-checking status...");

                    // Re-check status after EnableMapSync
                    status = IsRegistryValid();
                    Debug.WriteLine($"Registry status after EnableMapSync: {status}");

                    if (status == RegistryStatus.Correct)
                    {
                        NotificationHelpers.ShowSuccess("Registry Fixed", "Map synchronization enabled. Registry status is now correct.", TimeSpan.FromSeconds(3));
                    }
                    else
                    {
                        NotificationHelpers.ShowError("Registry Issue Persists", "Map sync was attempted, but the registry status remains incorrect.", TimeSpan.FromSeconds(4));
                    }
                }
                catch (Exception ex)
                {
                    NotificationHelpers.ShowError("Registry Fix Failed", $"An error occurred while trying to enable map sync: {ex.Message}", TimeSpan.FromSeconds(5));
                    // If EnableMapSync fails with an error, the status remains the same (MissingMapSync),
                    // and we do not attempt to get the path.
                    return string.Empty;
                }
            }

            // Now, if status is Correct, try to get the path
            if (status == RegistryStatus.Correct)
            {
                // Re-open the key as EnableMapSync might have changed it
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                using RegistryKey? key = baseKey.OpenSubKey(RegistryConstants.RA3GamePath, writable: false);
                string? pathValue = key?.GetValue(INSTALL_DIR_VALUE_NAME) as string;
                Debug.WriteLine($"Retrieved path from registry: {pathValue ?? "(null or empty)"}");

                if (!string.IsNullOrEmpty(pathValue))
                {
                    NotificationHelpers.ShowInformation("Game Path Found", "Successfully retrieved the RA3 installation path from the registry.", TimeSpan.FromSeconds(3));
                }
                else
                {
                    NotificationHelpers.ShowWarning("Path Retrieval Warning", "Registry status is correct, but the path value was empty or null.", TimeSpan.FromSeconds(4));
                }

                return pathValue ?? string.Empty;
            }
            else
            {
                Debug.WriteLine($"Game path not retrieved because registry status is: {status}");
                NotificationHelpers.ShowError("Game Path Not Found", $"Cannot retrieve RA3 path. Registry status is '{status}'. Please check the registry.", TimeSpan.FromSeconds(5));
                return string.Empty;
            }
        }

        public static void SetRA3Path(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                NotificationHelpers.ShowError("Invalid Path", GamePathIsNullOrEmptyMsg, TimeSpan.FromSeconds(4));
                return; // Do not throw, just return
            }

            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                using RegistryKey? subKey = baseKey.OpenSubKey(RegistryConstants.RA3GamePath, writable: true);

                if (subKey == null)
                {
                    // Key does not exist, create it
                    using RegistryKey newSubKey = baseKey.CreateSubKey(RegistryConstants.RA3GamePath, writable: true) ?? throw new InvalidOperationException($"Failed to create registry key '{RegistryConstants.RA3GamePath}' in HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node.");
                    newSubKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
                    Debug.WriteLine($"Value '{INSTALL_DIR_VALUE_NAME}' set to '{path}' in HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RegistryConstants.RA3GamePath}");
                    NotificationHelpers.ShowSuccess("Registry Key Created", $"Registry key and '{INSTALL_DIR_VALUE_NAME}' value created successfully.", TimeSpan.FromSeconds(3));
                }
                else
                {
                    // Key exists, just set the value
                    subKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
                    Debug.WriteLine($"Value '{INSTALL_DIR_VALUE_NAME}' updated to '{path}' in HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RegistryConstants.RA3GamePath}");
                    NotificationHelpers.ShowInformation("Registry Updated", $"Registry value '{INSTALL_DIR_VALUE_NAME}' updated successfully.", TimeSpan.FromSeconds(3));
                }
            }
            catch (UnauthorizedAccessException)
            {
                NotificationHelpers.ShowError("Access Denied", "Failed to set RA3 path in registry. Please run the launcher as Administrator.", TimeSpan.FromSeconds(5));
                // Do not throw, just show notification
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError(RegistryErrorTitle, $"An unexpected error occurred while setting the RA3 path: {ex.Message}", TimeSpan.FromSeconds(5));
                // Do not throw, just show notification
            }
        }

        /// <summary>
        /// Sets the language in the registry.
        /// </summary>
        /// <param name="gameFolderPath">Path to the game folder.</param>
        /// <param name="language">Desired language code (e.g., 'english', 'russian').</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool SetLanguage(string gameFolderPath, string language)
        {
            if (string.IsNullOrWhiteSpace(gameFolderPath) || string.IsNullOrWhiteSpace(language))
            {
                NotificationHelpers.ShowError("Invalid Input", "Game folder path and language cannot be null or empty.", TimeSpan.FromSeconds(4));
                return false;
            }

            language = language.ToLower();

            // Check if game files exist
            if (!Directory.EnumerateFiles(gameFolderPath, "*.skudef").Any())
            {
                NotificationHelpers.ShowError("Invalid Game Path", "The specified game folder does not appear to contain valid RA3 files.", TimeSpan.FromSeconds(4));
                return false;
            }

            // Determine the final language to set (check specified language, then fallback to default)
            string? finalLanguage = DetermineFinalLanguage(gameFolderPath, language);

            if (finalLanguage == null)
            {
                NotificationHelpers.ShowError("Language Not Found", $"Neither '{language}' nor '{DefaultLanguageName}' language files were found in the specified folder.", TimeSpan.FromSeconds(5));
                return false;
            }

            // Attempt to write to the registry
            try
            {
                WriteLanguageToRegistry(finalLanguage);
                return true; // Success
            }
            catch (UnauthorizedAccessException)
            {
                NotificationHelpers.ShowError("Admin Privileges Required", "Administrator privileges are required to modify the registry.", TimeSpan.FromSeconds(5));
                return false; // Indicate failure
            }
            catch (Exception ex) when (ex is SecurityException or IOException)
            {
                NotificationHelpers.ShowError("Language Setting Failed", $"Failed to set language in registry: {ex.Message}", TimeSpan.FromSeconds(5));
                return false; // Indicate failure
            }
        }

        /// <summary>
        /// Determines the final language code to use based on availability.
        /// </summary>
        /// <param name="gameFolderPath">Path to the game folder.</param>
        /// <param name="requestedLanguage">The initially requested language code.</param>
        /// <returns>The language code to use, or null if neither the requested nor the default is available.</returns>
        private static string? DetermineFinalLanguage(string gameFolderPath, string requestedLanguage)
        {
            // Check if the requested language files exist
            if (File.Exists(GetSkudefPath(gameFolderPath, requestedLanguage, "1.12")) && File.Exists(GetCsfPath(gameFolderPath, requestedLanguage)))
            {
                return requestedLanguage;
            }

            // Check if the default language files exist
            if (File.Exists(GetSkudefPath(gameFolderPath, DefaultLanguageName, "1.12")) && File.Exists(GetCsfPath(gameFolderPath, DefaultLanguageName)))
            {
                NotificationHelpers.ShowWarning("Language Fallback", $"Language '{requestedLanguage}' not found. Falling back to '{DefaultLanguageName}'.", TimeSpan.FromSeconds(4));
                return DefaultLanguageName;
            }

            // Neither the requested nor the default language files were found
            return null;
        }

        /// <summary>
        /// Writes the specified language code to the Windows registry under CurrentUser.
        /// </summary>
        /// <param name="languageCode">The language code to set (e.g., 'english', 'russian').</param>
        /// <exception cref="UnauthorizedAccessException">Thrown if insufficient permissions to modify the registry.</exception>
        /// <exception cref="SecurityException">Thrown for security-related issues.</exception>
        /// <exception cref="IOException">Thrown for I/O related issues.</exception>
        private static void WriteLanguageToRegistry(string languageCode)
        {
            using RegistryKey baseUserKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            using RegistryKey? userSubKey = baseUserKey.OpenSubKey(RegistryConstants.RA3GamePath, writable: true);

            if (userSubKey == null)
            {
                // Key does not exist, create it
                using RegistryKey newUserSubKey = baseUserKey.CreateSubKey(RegistryConstants.RA3GamePath, writable: true);
                if (newUserSubKey == null)
                {
                    Debug.WriteLine($"Failed to create registry key '{RegistryConstants.RA3GamePath}' in HKEY_CURRENT_USER to set language.");
                    NotificationHelpers.ShowError(RegistryErrorTitle, "Failed to create registry key for language setting.", TimeSpan.FromSeconds(4));
                    // Returning here instead of throwing
                    return;
                }
                newUserSubKey.SetValue(LANGUAGE_VALUE_NAME, languageCode, RegistryValueKind.String);
                Debug.WriteLine($"Value '{LANGUAGE_VALUE_NAME}' set to '{languageCode}' in HKEY_CURRENT_USER\\{RegistryConstants.RA3GamePath}");
            }
            else
            {
                // Key exists, set the value
                userSubKey.SetValue(LANGUAGE_VALUE_NAME, languageCode, RegistryValueKind.String);
                Debug.WriteLine($"Value '{LANGUAGE_VALUE_NAME}' updated to '{languageCode}' in HKEY_CURRENT_USER\\{RegistryConstants.RA3GamePath}");
            }
        }

        public static string? GetLanguage(string gameFolderPath)
        {
            if (string.IsNullOrWhiteSpace(gameFolderPath))
            {
                NotificationHelpers.ShowError("Invalid Input", "Game folder path cannot be null or empty.", TimeSpan.FromSeconds(4));
                return null; // Invalid input
            }

            try
            {
                // Check if the game folder contains at least one .skudef file
                bool isPathValid = Directory.EnumerateFiles(gameFolderPath, "*.skudef").Any();
                if (!isPathValid)
                {
                    NotificationHelpers.ShowError("Invalid Game Path", "The specified game folder does not appear to contain valid RA3 files.", TimeSpan.FromSeconds(4));
                    return null;
                }

                // Read the language value from the registry (CurrentUser)
                using RegistryKey baseUserKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
                using RegistryKey? userSubKey = baseUserKey.OpenSubKey(RegistryConstants.RA3GamePath, writable: false);

                if (userSubKey == null)
                {
                    // Key does not exist, return null
                    NotificationHelpers.ShowWarning("Language Not Set", "Language value not found in registry. Using default language.", TimeSpan.FromSeconds(4));
                    return null;
                }

                object? registryValue = userSubKey.GetValue(LANGUAGE_VALUE_NAME);
                string? currentLanguage = registryValue?.ToString();

                if (string.IsNullOrWhiteSpace(currentLanguage))
                {
                    // Language value is not set or empty
                    NotificationHelpers.ShowWarning("Language Not Set", "Language value in registry is empty. Using default language.", TimeSpan.FromSeconds(4));
                    return null;
                }

                // Check if files exist for the current language
                bool languageFilesExist = File.Exists(GetSkudefPath(gameFolderPath, currentLanguage, "1.12")) && File.Exists(GetCsfPath(gameFolderPath, currentLanguage));

                if (!languageFilesExist)
                {
                    // If files for the current language are not found, check English as fallback
                    if (File.Exists(GetSkudefPath(gameFolderPath, DefaultLanguageName, "1.12")) && File.Exists(GetCsfPath(gameFolderPath, DefaultLanguageName)))
                    {
                        // Language files exist for English, but the currently set language in registry is unavailable
                        // Returning null indicates the set language is invalid, and the caller might want to fall back.
                        NotificationHelpers.ShowWarning("Language Unavailable", $"Language '{currentLanguage}' set in registry is not available. English files are present.", TimeSpan.FromSeconds(5));
                        return null; // Or return DefaultLanguageName depending on desired logic
                    }
                    else
                    {
                        // Neither the current language nor English are available
                        NotificationHelpers.ShowError("Language Unavailable", $"Neither the language set in registry ('{currentLanguage}') nor '{DefaultLanguageName}' are available.", TimeSpan.FromSeconds(5));
                        return null;
                    }
                }

                return currentLanguage;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException or IOException)
            {
                Debug.WriteLine($"Error retrieving language from registry: {ex.Message}");
                NotificationHelpers.ShowError("Language Retrieval Failed", $"Failed to retrieve language from registry: {ex.Message}", TimeSpan.FromSeconds(5));
                return null; // Return null on access or IO errors
            }
        }

        private static string GetCsfPath(string gamePath, string lang)
        {
            return Path.Combine(gamePath, "Launcher", $"{lang}.csf");
        }

        private static string GetSkudefPath(string gamePath, string lang, string version)
        {
            return Path.Combine(gamePath, $"RA3_{lang}_{version}.skudef");
        }

        public static void EnableMaps()
        {
            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

                RegistryKey? subKey = baseKey.OpenSubKey(RegistryConstants.RA3GamePath, writable: true);

                if (subKey == null)
                {
                    // Key does not exist, create it
                    subKey = baseKey.CreateSubKey(RegistryConstants.RA3GamePath, writable: true) ?? throw new InvalidOperationException($"Failed to create registry key '{RegistryConstants.RA3GamePath}' in HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node.");

                    subKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
                    Debug.WriteLine($"Value '{USE_LOCAL_USER_MAP_VALUE_NAME}' set to 0 in HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RegistryConstants.RA3GamePath}");
                }
                else
                {
                    // Key exists, set the value
                    subKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
                    Debug.WriteLine($"Value '{USE_LOCAL_USER_MAP_VALUE_NAME}' updated to 0 in HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RegistryConstants.RA3GamePath}");
                }

                NotificationHelpers.ShowSuccess("Maps Enabled", "Map synchronization has been enabled in the registry.", TimeSpan.FromSeconds(3));
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException or IOException)
            {
                NotificationHelpers.ShowError("Admin Privileges Required", "Failed to enable maps in registry. Attempting to restart launcher as Administrator...", TimeSpan.FromSeconds(5));

                GamePatchesManager.RestartWithAdministratorPrivileges();
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError(RegistryErrorTitle, $"An error occurred while working with the registry: {ex.Message}", TimeSpan.FromSeconds(5));
                // Do not throw, just show notification
            }
        }

        public static void ResetRegistry(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                NotificationHelpers.ShowError("Invalid Path", GamePathIsNullOrEmptyMsg, TimeSpan.FromSeconds(4));
                return; // Do not throw, just return
            }

            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                using RegistryKey? subKey = baseKey.OpenSubKey(RegistryConstants.RA3GamePath, writable: true);

                if (subKey == null)
                {
                    // Key does not exist, create it
                    using RegistryKey newSubKey = baseKey.CreateSubKey(RegistryConstants.RA3GamePath, writable: true) ?? throw new InvalidOperationException($"Failed to create registry key '{RegistryConstants.RA3GamePath}' in HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node.");
                    newSubKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
                    newSubKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
                    Debug.WriteLine($"Key HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RegistryConstants.RA3GamePath} created with parameters Install Dir='{path}', UseLocalUserMap=0");
                    NotificationHelpers.ShowSuccess("Registry Reset", "Registry key created and reset successfully.", TimeSpan.FromSeconds(3));
                }
                else
                {
                    // Key exists, update values
                    subKey.SetValue(INSTALL_DIR_VALUE_NAME, path, RegistryValueKind.String);
                    subKey.SetValue(USE_LOCAL_USER_MAP_VALUE_NAME, 0, RegistryValueKind.DWord);
                    Debug.WriteLine($"Key HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\{RegistryConstants.RA3GamePath} updated: Install Dir='{path}', UseLocalUserMap=0");
                    NotificationHelpers.ShowInformation("Registry Updated", "Registry values reset successfully.", TimeSpan.FromSeconds(3));
                }
            }
            catch (UnauthorizedAccessException)
            {
                NotificationHelpers.ShowError("Access Denied", "Failed to reset registry. Please run the launcher as Administrator.", TimeSpan.FromSeconds(5));
                // Do not throw, just show notification
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError(RegistryErrorTitle, $"An unexpected error occurred while resetting the registry: {ex.Message}", TimeSpan.FromSeconds(5));
                // Do not throw, just show notification
            }
        }

        public static string? GetRA3BattleNetPath()
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegistryConstants.RA3BattleNetPath);
            if (key != null)
            {
                // The default value contains the executable path
                var path = key.GetValue(null) as string;
                if (!string.IsNullOrEmpty(path))
                {
                    NotificationHelpers.ShowInformation("BattleNet Path Found", "Successfully retrieved the RA3 BattleNet executable path from the registry.", TimeSpan.FromSeconds(3));
                }
                return path;
            }
            else
            {
                Debug.WriteLine($"Key {RegistryConstants.RA3BattleNetPath} not found.");
                return null;
            }
        }
    }
#pragma warning restore CA1416
}
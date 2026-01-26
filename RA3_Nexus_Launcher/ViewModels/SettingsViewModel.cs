using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Helpers;
using RA3_Nexus_Launcher.Managers;
using RA3_Nexus_Launcher.Models;
using RA3_Nexus_Launcher.Models.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA3_Nexus_Launcher.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private const string NoneIPValue = "0.0.0.0";

        [ObservableProperty]
        private List<WindowParam> _windowParams = [.. Enum.GetValues<WindowParam>()];

        [ObservableProperty]
        private WindowParam _selectedWindowParam = SettingsManager.CurrentSettings.WindowParam;

        [ObservableProperty]
        private bool _isCustomResoulution = SettingsManager.CurrentSettings.UseCustomResolution;

        [ObservableProperty]
        private string _customResolutionWidth = SettingsManager.CurrentSettings.Width.ToString();

        [ObservableProperty]
        private string _customResolutionHeight = SettingsManager.CurrentSettings.Height.ToString();

        [ObservableProperty]
        private string _selectedGamePath = SettingsManager.CurrentSettings.GamePath;

        [ObservableProperty]
        private List<string> _availableIPs = NetworkHelper.GetAllIPv4Addresses();

        [ObservableProperty]
        private List<GameProfile> _availableProfiles = GameProfilesHelper.GetProfilesList();

        [ObservableProperty]
        private string? _launchParameters = string.Join(' ', SettingsManager.CurrentSettings.LaunchParameters ?? []);

        [ObservableProperty]
        private GameProfile? _selectedProfile;

        [ObservableProperty]
        private string? _selectedGameSpyId;

        [ObservableProperty]
        private string? _selectedIp;

        [ObservableProperty]
        private List<string> _availableLanguages = [.. Directory
            .GetFiles(Path.Combine(SettingsManager.CurrentSettings.GameFolderPath, "Launcher"), "*.csf")
            .Select(csf => StringHelper.CapitalizeFirst(Path.GetFileNameWithoutExtension(csf)))];

        [ObservableProperty]
        private string? _selectedLanguage;

        [ObservableProperty]
        private List<string> _availableVersions = ["1.13", "1.12", "1.11", "1.10", "1.9", "1.8", "1.7", "1.6", "1.5", "1.4", "1.3", "1.2", "1.1", "1.0"];

        [ObservableProperty]
        private string _selectedVersion = SettingsManager.CurrentSettings.RunVersion;

        // --- Новое свойство ---
        [ObservableProperty]
        private SettingsViewType _currentViewType = SettingsViewType.GlobalSettings;

        public SettingsViewModel()
        {
            AvailableIPs.Add("None");

            InitializeLanguageSettings();
            InitializeProfileSettings();
        }

        private void InitializeLanguageSettings()
        {
            if (AvailableLanguages.Count > 0)
            {
                string? lang = GameRegistryHelper.GetLanguage(SettingsManager.CurrentSettings.GameFolderPath);

                if (!string.IsNullOrWhiteSpace(lang))
                {
                    SelectedLanguage = StringHelper.CapitalizeFirst(lang);
                }
                else
                {
                    SelectedLanguage = AvailableLanguages[0];
                    ApplyLanguage();
                }
            }
        }

        private void InitializeProfileSettings()
        {
            if (AvailableProfiles.Count == 0)
            {
                return;
            }

            SelectedProfile = AvailableProfiles[0];

            if (SelectedProfile.HasValue)
            {
                string? gameSpyId = SelectedProfile.Value.GameSpyIdAddress;
                string? id = SelectedProfile.Value.IPAddress;
                SelectedGameSpyId = string.IsNullOrWhiteSpace(gameSpyId) || gameSpyId == NoneIPValue ? "None" : gameSpyId;
                SelectedIp = string.IsNullOrWhiteSpace(id) || id == NoneIPValue ? "None" : id;
            }
            else
            {
                SelectedGameSpyId = "None";
                SelectedIp = "None";
            }
        }

        public static string ReadMoreIconData => PathIconConstants.ReadMoreIconData;
        public static string SettingsIconData => PathIconConstants.SettingsIconData;
        public static string SyncArrowIconData => PathIconConstants.SyncArrowIconData;
        public static string OpenFolderIconData => PathIconConstants.OpenFolderIconData;

        partial void OnSelectedVersionChanged(string value)
        {
            SettingsManager.CurrentSettings.RunVersion = value;
            SettingsManager.SaveCurrentSettings();
        }

        partial void OnSelectedGamePathChanged(string value)
        {
            SettingsManager.CurrentSettings.GameFolderPath = Path.GetDirectoryName(value) ?? string.Empty;
            SettingsManager.SaveCurrentSettings();
        }

        partial void OnIsCustomResoulutionChanged(bool value)
        {
            SettingsManager.CurrentSettings.UseCustomResolution = value;
            SettingsManager.SaveCurrentSettings();
        }

        partial void OnSelectedWindowParamChanged(WindowParam value)
        {
            SettingsManager.CurrentSettings.WindowParam = value;
            SettingsManager.SaveCurrentSettings();
        }

        // --- Новые команды для переключения вкладок ---
        [RelayCommand]
        private void ShowGlobalSettings()
        {
            CurrentViewType = SettingsViewType.GlobalSettings;
        }

        [RelayCommand]
        private void ShowPatches()
        {
            CurrentViewType = SettingsViewType.Patches;
        }

        [RelayCommand]
        private static void OpenAllParameters()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = PathConstants.RA3LaunchParametersTxt,
                    UseShellExecute = true // Очень важно для открытия файлов через ассоциацию
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка открытия файла: {ex.Message} {ex.InnerException}");
            }
        }

        [RelayCommand]
        private void ReloadGamePath()
        {
            string gameFolderPath = GameHelper.GetGameFolder();
            SettingsManager.CurrentSettings.GameFolderPath = gameFolderPath;
            SettingsManager.SaveCurrentSettings();

            SelectedGamePath = SettingsManager.CurrentSettings.GamePath;
        }

        [RelayCommand]
        private async Task SelectGamePath()
        {
            IStorageFile? file = await DialogsManager.ShowOpenSingleFileDialogAsync("Select RA3.exe", ["RA3.exe"]);

            if (file != null)
            {
                string filePath = file.Path.LocalPath;

                if (File.Exists(filePath))
                {
                    string folderPath = Path.GetDirectoryName(filePath)!;

                    SettingsManager.CurrentSettings.GameFolderPath = folderPath;
                    SettingsManager.SaveCurrentSettings();

                    SelectedGamePath = SettingsManager.CurrentSettings.GamePath;
                    GameRegistryHelper.SetRA3Path(filePath);
                }
                else
                {
                    ReloadGamePath();
                }
            }
            else
            {
                ReloadGamePath();
            }
        }

        partial void OnSelectedProfileChanged(GameProfile? value)
        {
            if (value.HasValue)
            {
                string? gameSpyId = value.Value.GameSpyIdAddress;
                string? ipAddress = value.Value.IPAddress;

                SelectedGameSpyId = string.IsNullOrWhiteSpace(gameSpyId) || gameSpyId == NoneIPValue ? "None" : gameSpyId;
                SelectedIp = string.IsNullOrWhiteSpace(ipAddress) || ipAddress == NoneIPValue ? "None" : ipAddress;
            }
            else
            {
                SelectedGameSpyId = "None";
                SelectedIp = "None";
            }
        }

        partial void OnLaunchParametersChanged(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                SettingsManager.CurrentSettings.LaunchParameters = null;
            }
            else
            {
                // Разбиваем строку по пробелам, но учитываем возможные кавычки
                var parameters = ParseLaunchParameters(value);
                SettingsManager.CurrentSettings.LaunchParameters = parameters;
            }
            SettingsManager.SaveCurrentSettings();
        }

        private static string[] ParseLaunchParameters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Array.Empty<string>();
            }

            var parameters = new List<string>();
            var parts = input.Split('-', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    // Восстанавливаем '-' в начале параметра
                    var param = "-" + trimmedPart;
                    parameters.Add(param);
                }
            }

            return parameters.ToArray();
        }

        [RelayCommand]
        private void ApplyLanguage()
        {
            if (!string.IsNullOrWhiteSpace(SelectedLanguage))
            {
                GameRegistryHelper.SetLanguage(SettingsManager.CurrentSettings.GameFolderPath, SelectedLanguage.Trim());
            }
        }

        [RelayCommand]
        private void ApplyIpSettings()
        {
            if (SelectedProfile != null)
            {
                string? gameSpyIp = SelectedGameSpyId == "None" ? NoneIPValue : SelectedGameSpyId;
                string? ipAddress = SelectedIp == "None" ? NoneIPValue : SelectedIp;

                GameProfilesHelper.UpdateProfileIpAddresses(SelectedProfile.Value.OptionsPath, gameSpyIp, ipAddress);
            }
        }

        [RelayCommand]
        private void FixSkirmishFile()
        {
            if (AvailableProfiles.Count > 0)
            {
                GameProfilesHelper.CheckAndFixSkirmish();
            }
        }

        [RelayCommand]
        private static void EnableMapsInRegistry()
        {
            GameHelper.EnableMaps();
        }

        [RelayCommand]
        private static void InstallFourGBPatch()
        {
            GamePatchesManager.InstallFourGBPatch();
        }

        [RelayCommand]
        private static void GenerateCDKey()
        {
            GamePatchesManager.GenerateNewCDKey();
        }

        [RelayCommand]
        private static void FixRegistry()
        {
            GamePatchesManager.FixRegistry();
        }

        [RelayCommand]
        private static void OpenRA3BattleNetUrl()
        {
            UrlHelper.OpenUrl(UrlConstants.BattleNetUrl);
        }

        [RelayCommand]
        private static void OpenTacitusUrl()
        {
            UrlHelper.OpenUrl(UrlConstants.TacitusUrl);
        }

        [RelayCommand]
        private static void OpenRadminVPNUrl()
        {
            UrlHelper.OpenUrl(UrlConstants.RadminVPNUrl);
        }
    }
}
using Avalonia.Controls; // Added for Window
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RA3_Nexus_Launcher.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private const string NoneIPValue = "0.0.0.0";
        private const string DefaultVolume = "100.000000";
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

        // Changed: use NetworkInterfaceInfo instead of List<string>
        [ObservableProperty]
        private List<NetworkInterfaceInfo> _availableIPs = NetworkHelper.GetNetworkInterfaceInfo();

        [ObservableProperty]
        private List<GameProfile> _availableProfiles = GameProfilesManager.GetProfilesList();

        [ObservableProperty]
        private string? _launchParameters = string.Join(' ', SettingsManager.CurrentSettings.LaunchParameters ?? []);

        [ObservableProperty]
        private GameProfile? _selectedProfile;

        // Changed: use NetworkInterfaceInfo instead of string?
        [ObservableProperty]
        private NetworkInterfaceInfo _selectedGameSpyId;

        // Changed: use NetworkInterfaceInfo instead of string?
        [ObservableProperty]
        private NetworkInterfaceInfo _selectedIp;

        // --- New properties for volume ---
        [ObservableProperty]
        private string _ambientVolumeStr = DefaultVolume;

        [ObservableProperty]
        private string _movieVolumeStr = DefaultVolume;

        [ObservableProperty]
        private string _musicVolumeStr = DefaultVolume;

        [ObservableProperty]
        private string _sfxVolumeStr = DefaultVolume;

        [ObservableProperty]
        private string _voiceVolumeStr = DefaultVolume;

        [ObservableProperty]
        private ObservableCollection<string> _availableLanguages = [.. Directory
            .GetFiles(Path.Combine(SettingsManager.CurrentSettings.GameFolder, "Launcher"), "*.csf")
            .Select(csf => StringHelper.CapitalizeFirst(Path.GetFileNameWithoutExtension(csf)))];

        [ObservableProperty]
        private string? _selectedLanguage;

        [ObservableProperty]
        private List<string> _availableVersions = ["1.13", "1.12", "1.11", "1.10", "1.9", "1.8", "1.7", "1.6", "1.5", "1.4", "1.3", "1.2", "1.1", "1.0"];

        [ObservableProperty]
        private string _selectedVersion = SettingsManager.CurrentSettings.RunVersion;

        [ObservableProperty]
        private SettingsViewType _currentViewType = SettingsViewType.GlobalSettings;

        [ObservableProperty]
        private bool _isArgumentsVisible = false;

        // Changed: store reference to Window
        private readonly Window? _appWindow;

        private double AmbientVolumeForSaving => ParseAndClampVolume(AmbientVolumeStr);
        private double MovieVolumeForSaving => ParseAndClampVolume(MovieVolumeStr);
        private double MusicVolumeForSaving => ParseAndClampVolume(MusicVolumeStr);
        private double SFXVolumeForSaving => ParseAndClampVolume(SfxVolumeStr);
        private double VoiceVolumeForSaving => ParseAndClampVolume(VoiceVolumeStr);

        private static double ParseAndClampVolume(string input)
        {
            if (double.TryParse(input, out double value))
            {
                // Limit from 0.0 to 100.0
                return Math.Max(0.0, Math.Min(100.0, value));
            }

            // If the string is not numeric, return 0.0 as a safe value
            return 0.0;
        }

        // Changed: constructor takes Window
        public SettingsViewModel(Window appWindow)
        {
            _appWindow = appWindow; // Store reference
            // Add "None" as NetworkInterfaceInfo
            AvailableIPs.Add(new NetworkInterfaceInfo(NoneIPValue, "None"));

            InitializeLanguageSettings();
            InitializeProfileSettings();
            LoadAudioSettingsForSelectedProfile();
        }

        [RelayCommand]
        private void CloseWindow()
        {
            _appWindow?.Close();
        }

        private void InitializeLanguageSettings()
        {
            if (AvailableLanguages.Count == 0)
            {
                NotificationHelpers.ShowWarning("No Languages Found", "No language files (.csf) were found in the game's Launcher folder.", TimeSpan.FromSeconds(4));
            }
            else
            {
                string? currentLanguage = GameRegistryHelper.GetLanguage(SettingsManager.CurrentSettings.GameFolder);

                if (!string.IsNullOrWhiteSpace(currentLanguage))
                {
                    SelectedLanguage = StringHelper.CapitalizeFirst(currentLanguage);
                }
            }
        }

        private void InitializeProfileSettings()
        {
            if (AvailableProfiles.Count == 0)
            {
                NotificationHelpers.ShowWarning("No Profiles", "No game profiles were found. Some settings may not be applicable.", TimeSpan.FromSeconds(4));
                return;
            }

            SelectedProfile = AvailableProfiles[0];

            if (SelectedProfile.HasValue)
            {
                string? gameSpyId = SelectedProfile.Value.GameSpyIdAddress;
                string? id = SelectedProfile.Value.IPAddress;
                // Find the corresponding NetworkInterfaceInfo or use "None"
                SelectedGameSpyId = FindNetworkInterfaceInfoByIp(gameSpyId) ?? new NetworkInterfaceInfo(NoneIPValue, "None");
                SelectedIp = FindNetworkInterfaceInfoByIp(id) ?? new NetworkInterfaceInfo(NoneIPValue, "None");
            }
            else
            {
                SelectedGameSpyId = new NetworkInterfaceInfo(NoneIPValue, "None");
                SelectedIp = new NetworkInterfaceInfo(NoneIPValue, "None");
            }
        }

        // Helper method to find NetworkInterfaceInfo by IP
        private NetworkInterfaceInfo? FindNetworkInterfaceInfoByIp(string? targetIp)
        {
            if (string.IsNullOrEmpty(targetIp) || targetIp == NoneIPValue)
            {
                return null; // Returns "None" element
            }

            foreach (NetworkInterfaceInfo info in AvailableIPs)
            {
                if (info.IpAddress == targetIp)
                {
                    return info;
                }
            }
            return null;
        }


        // Loads audio settings for the selected profile
        private void LoadAudioSettingsForSelectedProfile()
        {
            if (SelectedProfile.HasValue)
            {
                var optionsFilePath = SelectedProfile.Value.OptionsPath;
                if (File.Exists(optionsFilePath))
                {
                    var (ambientVol, movieVol, musicVol, sfxVol, voiceVol) = IniFileHelper.ReadAudioVolumes(optionsFilePath);
                    // Set string properties using double values directly (they are already percentages!)
                    // If value is null, use 100.0 as default
                    AmbientVolumeStr = (ambientVol ?? 100.0).ToString("F6");
                    MovieVolumeStr = (movieVol ?? 100.0).ToString("F6");
                    MusicVolumeStr = (musicVol ?? 100.0).ToString("F6");
                    SfxVolumeStr = (sfxVol ?? 100.0).ToString("F6");
                    VoiceVolumeStr = (voiceVol ?? 100.0).ToString("F6");
                }
                else
                {
                    // If file doesn't exist, reset to default values (100%)
                    AmbientVolumeStr = DefaultVolume;
                    MovieVolumeStr = DefaultVolume;
                    MusicVolumeStr = DefaultVolume;
                    SfxVolumeStr = DefaultVolume;
                    VoiceVolumeStr = DefaultVolume;
                    NotificationHelpers.ShowWarning("Audio File Missing", $"Options.ini not found for profile: {SelectedProfile.Value.Name}. Using default values.", TimeSpan.FromSeconds(4));
                }
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
            SettingsManager.CurrentSettings.GameFolder = Path.GetDirectoryName(value) ?? string.Empty;
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

        // Handle the change of the selected profile
        partial void OnSelectedProfileChanged(GameProfile? value)
        {
            if (value.HasValue && SelectedProfile.HasValue && SelectedProfile.Value.ProfilePath != value.Value.ProfilePath)
            {
                SaveAudioSettingsForSelectedProfile();
            }

            if (value.HasValue)
            {
                string? gameSpyId = value.Value.GameSpyIdAddress;
                string? ipAddress = value.Value.IPAddress;

                // Find corresponding NetworkInterfaceInfo
                SelectedGameSpyId = FindNetworkInterfaceInfoByIp(gameSpyId) ?? new NetworkInterfaceInfo(NoneIPValue, "None");
                SelectedIp = FindNetworkInterfaceInfoByIp(ipAddress) ?? new NetworkInterfaceInfo(NoneIPValue, "None");

                // Load audio settings for the new profile
                LoadAudioSettingsForSelectedProfile();
            }
            else
            {
                SelectedGameSpyId = new NetworkInterfaceInfo(NoneIPValue, "None");
                SelectedIp = new NetworkInterfaceInfo(NoneIPValue, "None");

                AmbientVolumeStr = DefaultVolume;
                MovieVolumeStr = DefaultVolume;
                MusicVolumeStr = DefaultVolume;
                SfxVolumeStr = DefaultVolume;
                VoiceVolumeStr = DefaultVolume;
            }
        }

        // Non-static method to save current ViewModel values to the selected profile
        // Called when TextBox values change or when "Apply" is clicked
        private void SaveAudioSettingsForSelectedProfile()
        {
            if (!SelectedProfile.HasValue)
            {
                // Nothing to save if no profile is selected
                return;
            }

            GameProfile profile = SelectedProfile.Value;
            var optionsFilePath = profile.OptionsPath;

            if (!File.Exists(optionsFilePath))
            {
                NotificationHelpers.ShowError("File Not Found", $"Options.ini file not found for profile '{profile.Name}': {optionsFilePath}", TimeSpan.FromSeconds(5));
                return;
            }

            // Use computed properties from ViewModel to get values (already in 0.0 - 100.0 format)
            // and pass them directly to IniFileHelper (assuming IniFileHelper expects percentages as double).
            IniFileHelper.WriteAudioVolumes(
                optionsFilePath,
                AmbientVolumeForSaving, // Value is already in percentages (0.0 - 100.0)
                MovieVolumeForSaving,
                MusicVolumeForSaving,
                SFXVolumeForSaving,
                VoiceVolumeForSaving
            );


            // Optional: update AudioSettings in the profile object in AvailableProfiles list,
            // if it's used elsewhere for display or other operations.
            var index = AvailableProfiles.FindIndex(p => p.Name == profile.Name);
            if (index != -1)
            {
                GameProfile updatedProfile = AvailableProfiles[index];
                updatedProfile.AudioSettings = new AudioSettings
                {
                    AmbientVolume = AmbientVolumeForSaving, // Update values in profile too
                    MovieVolume = MovieVolumeForSaving,
                    MusicVolume = MusicVolumeForSaving,
                    SFXVolume = SFXVolumeForSaving,
                    VoiceVolume = VoiceVolumeForSaving
                };
                AvailableProfiles[index] = updatedProfile; // Requires ObservableCollection for notifications
            }
        }

        partial void OnAmbientVolumeStrChanged(string value)
        {
            if (SelectedProfile.HasValue)
            {
                SaveAudioSettingsForSelectedProfile();
            }
        }

        partial void OnMovieVolumeStrChanged(string value)
        {
            if (SelectedProfile.HasValue)
            {
                SaveAudioSettingsForSelectedProfile();
            }
        }

        partial void OnMusicVolumeStrChanged(string value)
        {
            if (SelectedProfile.HasValue)
            {
                SaveAudioSettingsForSelectedProfile();
            }
        }

        partial void OnSfxVolumeStrChanged(string value)
        {
            if (SelectedProfile.HasValue)
            {
                SaveAudioSettingsForSelectedProfile();
            }
        }

        partial void OnVoiceVolumeStrChanged(string value)
        {
            if (SelectedProfile.HasValue)
            {
                SaveAudioSettingsForSelectedProfile();
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
                var parameters = ParseLaunchParameters(value);
                SettingsManager.CurrentSettings.LaunchParameters = parameters;
            }
            SettingsManager.SaveCurrentSettings();
        }

        private static string[] ParseLaunchParameters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return [];
            }

            var parameters = new List<string>();
            var parts = input.Split('-', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    var param = "-" + trimmedPart;
                    parameters.Add(param);
                }
            }

            return [.. parameters];
        }

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
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening file: {ex.Message} {ex.InnerException}");
                NotificationHelpers.ShowError("Error opening file", $"{ex.Message} {ex.InnerException}", TimeSpan.FromSeconds(5));
            }
        }

        [RelayCommand]
        private static void ReloadGamePath()
        {
            string gameFolderPath = GameHelper.GetGameFolder();
            if (string.IsNullOrEmpty(gameFolderPath))
            {
                NotificationHelpers.ShowError("Path Not Found", "Could not auto-detect RA3 installation path.", TimeSpan.FromSeconds(4));
            }
        }

        [RelayCommand]
        private async Task SelectGamePath()
        {
            IStorageFile? file = await DialogsManager.ShowOpenSingleFileDialogAsync("Select RA3.exe", ["RA3.exe"]);

            if (file != null)
            {
                string filePath = file.Path.LocalPath;

                if (!File.Exists(filePath))
                {
                    ReloadGamePath();
                    NotificationHelpers.ShowWarning("File Missing", "Selected RA3.exe not found, reloaded path.", TimeSpan.FromSeconds(3));
                }
                else
                {
                    string folderPath = Path.GetDirectoryName(filePath)!;

                    SettingsManager.CurrentSettings.GameFolder = folderPath;
                    SettingsManager.SaveCurrentSettings();

                    SelectedGamePath = SettingsManager.CurrentSettings.GamePath;
                    GameRegistryHelper.SetRA3Path(filePath);
                }
            }
            else
            {
                ReloadGamePath();
                NotificationHelpers.ShowWarning("Selection Cancelled", "Game path selection cancelled, reloaded path.", TimeSpan.FromSeconds(3));
            }
        }


        [RelayCommand]
        private void ApplyLanguage()
        {
            if (string.IsNullOrWhiteSpace(SelectedLanguage))
            {
                NotificationHelpers.ShowError("Invalid Language", "Selected language is empty or invalid.", TimeSpan.FromSeconds(4));
                return;
            }

            try
            {
                bool success = GameRegistryHelper.SetLanguage(SettingsManager.CurrentSettings.GameFolder, SelectedLanguage.Trim());
                if (success)
                {
                    NotificationHelpers.ShowSuccess("Language Set", $"Game language successfully set to '{SelectedLanguage.Trim()}'.", TimeSpan.FromSeconds(3));
                }
            }
            catch (InvalidOperationException ex) when (ex.InnerException is UnauthorizedAccessException)
            {
                // Это наш случай: нужны права администратора
                NotificationHelpers.ShowError("Admin Privileges Required", "Failed to set language. The launcher needs Administrator privileges to modify the Windows Registry.", TimeSpan.FromSeconds(5));

                // Запрашиваем перезапуск от имени администратора
                var process = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath!,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                try
                {
                    NotificationHelpers.ShowInformation("Restart Initiated", "A new instance of the launcher will start with Administrator privileges. You can close this window.", TimeSpan.FromSeconds(4));
                    Process.Start(process);
                    Environment.Exit(0);
                }
                catch (Exception startupEx)
                {
                    Debug.WriteLine($"Failed to restart as admin: {startupEx.Message}");
                    NotificationHelpers.ShowError("Restart Failed", $"Could not restart the launcher as Administrator: {startupEx.Message}", TimeSpan.FromSeconds(5));
                }
            }
            catch (InvalidOperationException ex)
            {
                // Другие ошибки (например, IO)
                NotificationHelpers.ShowError("Language Setting Failed", ex.Message, TimeSpan.FromSeconds(5));
            }
        }

        [RelayCommand]
        private void ApplyIpSettings()
        {
            if (!SelectedProfile.HasValue)
            {
                NotificationHelpers.ShowWarning("No Profile", "Cannot apply IP settings: no profile selected.", TimeSpan.FromSeconds(4));
                return;
            }

            // Take IP address from SelectedGameSpyId/SelectedIp structure
            string? gameSpyIp = SelectedGameSpyId.IpAddress == NoneIPValue ? "None" : SelectedGameSpyId.IpAddress;
            string? ipAddress = SelectedIp.IpAddress == NoneIPValue ? "None" : SelectedIp.IpAddress;

            GameProfile newGameProfile = SelectedProfile.Value;
            newGameProfile.GameSpyIdAddress = gameSpyIp;
            newGameProfile.IPAddress = ipAddress;

            GameProfilesManager.UpdateProfileIpAddresses(newGameProfile);
        }

        [RelayCommand]
        private void FixSkirmishFile()
        {
            if (AvailableProfiles.Count == 0)
            {
                NotificationHelpers.ShowWarning("No Profiles", "No profiles found to fix skirmish files.", TimeSpan.FromSeconds(4));
            }
        }

        [RelayCommand]
        private static void EnableMapsInRegistry()
        {
            try
            {
                GameHelper.EnableMaps();
                // NotificationHelpers.ShowSuccess("Maps Enabled", "Map synchronization enabled in the registry.", TimeSpan.FromSeconds(3)); // Это уже делает GameHelper
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Enable Maps Failed", $"Error enabling maps in registry: {ex.Message}", TimeSpan.FromSeconds(5));
            }
        }

        [RelayCommand]
        private static void InstallFourGBPatch()
        {
            try
            {
                GamePatchesManager.InstallFourGBPatch();
                NotificationHelpers.ShowSuccess("Patch Installed", "4GB Patch installed successfully.", TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Install Failed", $"Error installing 4GB Patch: {ex.Message}", TimeSpan.FromSeconds(5));
            }
        }

        [RelayCommand]
        private static void GenerateCDKey()
        {
            try
            {
                string newCdKey = GamePatchesManager.GenerateNewCDKey();
                NotificationHelpers.ShowSuccess("CD Key Generated", $"New unique CD key generated:\n{newCdKey}", TimeSpan.FromSeconds(5)); // Показываем сгенерированный ключ
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Generate Failed", $"Error generating CD key: {ex.Message}", TimeSpan.FromSeconds(5));
            }
        }

        [RelayCommand]
        private static void FixRegistry()
        {
            try
            {
                GamePatchesManager.FixRegistry();
                NotificationHelpers.ShowSuccess("Registry Fixed", "Common registry errors fixed.", TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Fix Failed", $"Error fixing registry: {ex.Message}", TimeSpan.FromSeconds(5));
            }
        }

        [RelayCommand]
        private static void OpenRA3BattleNetUrl()
        {
            try
            {
                UrlHelper.OpenUrl(UrlConstants.BattleNetUrl);
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Open Failed", $"Error opening BattleNet URL: {ex.Message}", TimeSpan.FromSeconds(4));
            }
        }

        [RelayCommand]
        private static void OpenTacitusUrl()
        {
            try
            {
                UrlHelper.OpenUrl(UrlConstants.TacitusUrl);
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Open Failed", $"Error opening Tacitus URL: {ex.Message}", TimeSpan.FromSeconds(4));
            }
        }

        [RelayCommand]
        private static void OpenRadminVPNUrl()
        {
            try
            {
                UrlHelper.OpenUrl(UrlConstants.RadminVPNUrl);
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Open Failed", $"Error opening Radmin VPN URL: {ex.Message}", TimeSpan.FromSeconds(4));
            }
        }
    }
}
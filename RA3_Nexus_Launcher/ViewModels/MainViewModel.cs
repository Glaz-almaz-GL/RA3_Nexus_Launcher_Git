using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Helpers;
using RA3_Nexus_Launcher.Managers;
using RA3_Nexus_Launcher.Models;
using RA3_Nexus_Launcher.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;

namespace RA3_Nexus_Launcher.ViewModels;
#pragma warning disable CA1416 // Проверка совместимости платформы
public partial class MainViewModel(Window appWindow) : ViewModelBase
{
    public static string CurrentVersion => $"v{AppInfo.CurrentVersion}";
    public static string BattleNetIconData => PathIconConstants.BattleNetIconData;
    public static string DiscordIconData => PathIconConstants.DiscordIconData;
    public static string GithubIconData => PathIconConstants.GithubIconData;
    public static string ModDbIconData => PathIconConstants.ModDbIconData;
    public static string SettingsIconData => PathIconConstants.SettingsIconData;
    public static List<InstalledModInfo> InstalledMods => InstalledModsManager.InstalledMods;
    public static bool IsAdministratorMode => IsAdministrator();
    public static string? BattleNetPath => SettingsManager.CurrentSettings.BattleNetPath;
    private readonly UpdateCheckerService _updateCheckerService = new(
        new System.Net.Http.HttpClient(),
        "Glaz-almaz-GL",
        "RA3_Nexus_Launcher",
        SettingsManager.CurrentSettings.InstalledVersion.ToString());

    [ObservableProperty]
    private bool _isBattleNetAvailable = !string.IsNullOrWhiteSpace(BattleNetPath);

    private readonly Window? _appWindow = appWindow;
    [ObservableProperty]
    private InstalledModInfo? _selectedMod;

    public async Task InitializeAsync()
    {
        if (IsAdministratorMode)
        {
            NotificationHelpers.ShowSuccess("Successful launch", "Launch with administrator privileges successful", TimeSpan.FromSeconds(5));
        }

        if (!SettingsManager.CurrentSettings.IsQuickLoaderUsed)
        {
            NotificationHelpers.ShowInformation("RA3 QuickLauncher is not used (Click this post to use RA3 QuickLoader).",
                "RA3 QuickLoader - This is an improved version of RA3.exe (the original game executable file) for quickly launching the game (without waiting up to 30 seconds)",
                TimeSpan.FromSeconds(10),
                () => GamePatchesManager.ApplyQuickLoader());
        }

        await _updateCheckerService.CheckForUpdatesAsync();
    }

    [RelayCommand]
    private void CloseWindow()
    {
        _appWindow?.Close();
    }

    // Команда для сворачивания окна
    [RelayCommand]
    private void MinimizeWindow()
    {
        _appWindow?.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    private void RemoveSelectedMod()
    {
        SelectedMod = null;
    }

    [RelayCommand]
    private void StartGame()
    {
        MinimizeWindow();
        GameHelper.StartGame(SelectedMod);
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var settingsWindow = new SettingsWindow();

        if (_appWindow != null)
        {
            settingsWindow.ShowDialog(_appWindow);
            NotificationHelpers.SetNotificationManager(settingsWindow);
        }

        settingsWindow.Closing += Settings_Closing;
    }

    private void Settings_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (_appWindow != null)
        {
            NotificationHelpers.SetNotificationManager(_appWindow);
        }
    }

    [RelayCommand]
    private void StartBattleNet()
    {
        if (IsBattleNetAvailable)
        {
            Process.Start(BattleNetPath!);
            NotificationHelpers.ShowInformation(string.Empty, "RA3BattleNet has been successfully launched", TimeSpan.FromSeconds(3));
        }
    }

    [RelayCommand]
    private static void OpenGithub()
    {
        UrlHelper.OpenUrl(UrlConstants.GithubUrl);
    }

    [RelayCommand]
    private static void OpenDiscordChannel()
    {
        UrlHelper.OpenUrl(UrlConstants.DiscordUrl);
    }

    [RelayCommand]
    private static void OpenModDb()
    {
        UrlHelper.OpenUrl(UrlConstants.ModDbUrl);
    }

    public static bool IsAdministrator()
    {
        try
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}
#pragma warning restore CA1416 // Проверка совместимости платформы
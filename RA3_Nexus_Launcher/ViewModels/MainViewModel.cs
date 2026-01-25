using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Helpers;
using RA3_Nexus_Launcher.Managers;
using RA3_Nexus_Launcher.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RA3_Nexus_Launcher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public static string CurrentVersion => $"v{AppInfo.CurrentVersion}";
    public static string BattleNetIconData => PathIconConstants.BattleNetIconData;
    public static string DiscordIconData => PathIconConstants.DiscordIconData;
    public static string GithubIconData => PathIconConstants.GithubIconData;
    public static string ModDbIconData => PathIconConstants.ModDbIconData;
    public static string SettingsIconData => PathIconConstants.SettingsIconData;
    public static List<InstalledModInfo> InstalledMods => InstalledModsManager.InstalledMods;

    [ObservableProperty]
    private InstalledModInfo? _selectedMod;

    [RelayCommand]
    private static void CloseWindow()
    {
        Window? window = GetThisWindow();
        window?.Close();
    }

    // Команда для сворачивания окна
    [RelayCommand]
    private static void MinimizeWindow()
    {
        Window? window = GetThisWindow();
        window?.WindowState = WindowState.Minimized;
    }

    private static Window? GetThisWindow()
    {
        // Получаем активное окно (может не работать корректно, если открыто несколько окон)
        return App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }

    [RelayCommand]
    private void StartGame()
    {
        MinimizeWindow();
        GameHelper.StartGame(SelectedMod);
    }

    [RelayCommand]
    private static void OpenGithub()
    {
        OpenUrl(UrlConstants.GithubUrl);
    }

    [RelayCommand]
    private static void OpenDiscordChannel()
    {
        OpenUrl(UrlConstants.DiscordUrl);
    }

    [RelayCommand]
    private static void OpenModDb()
    {
        OpenUrl(UrlConstants.ModDbUrl);
    }

    /// <summary>
    /// Открывает указанный URL-адрес в браузере по умолчанию операционной системы.
    /// </summary>
    /// <param name="url">URL-адрес для открытия (например, "https://www.example.com").</param>
    public static void OpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL сannot be null or empty.", nameof(url));
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true // Важно для открытия URL через системный браузер
        });
    }
}

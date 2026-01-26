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

namespace RA3_Nexus_Launcher.ViewModels;
#pragma warning disable CA1416 // Проверка совместимости платформы
public partial class MainViewModel : ViewModelBase
{
    public static string CurrentVersion => $"v{AppInfo.CurrentVersion}";
    public static string BattleNetIconData => PathIconConstants.BattleNetIconData;
    public static string DiscordIconData => PathIconConstants.DiscordIconData;
    public static string GithubIconData => PathIconConstants.GithubIconData;
    public static string ModDbIconData => PathIconConstants.ModDbIconData;
    public static string SettingsIconData => PathIconConstants.SettingsIconData;
    public static List<InstalledModInfo> InstalledMods => InstalledModsManager.InstalledMods;
    public static bool IsAdministratorMode => IsAdministrator();

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

    [RelayCommand]
    private void RemoveSelectedMod()
    {
        SelectedMod = null;
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
    private static void OpenSettings()
    {
        Window? window = GetThisWindow();


        if (window != null)
        {
            new SettingsView().ShowDialog(window);
        }
    }

    [RelayCommand]
    private void StartBattleNet()
    {
        // TODO: Добавить функцию запуска RA3 BattleNet или убрать её
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
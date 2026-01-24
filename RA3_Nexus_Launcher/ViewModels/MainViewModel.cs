using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;

namespace RA3_Nexus_Launcher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
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
}

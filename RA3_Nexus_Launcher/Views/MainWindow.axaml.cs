using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading; // Добавьте этот using
using Huskui.Avalonia.Controls; // Если используется
using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Helpers;
using RA3_Nexus_Launcher.Managers;
using RA3_Nexus_Launcher.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RA3_Nexus_Launcher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var mainViewModel = new MainViewModel(this);
        DataContext = mainViewModel;

        InitializeComponent();
        DialogsManager.Initialize(this);
        NotificationHelpers.SetNotificationManager(this);
        SkirmishFixer.CheckAndFixSkirmish();

        // Запускаем InitializeAsync в контексте UI-потока
        Dispatcher.UIThread.InvokeAsync(mainViewModel.InitializeAsync, DispatcherPriority.Background);
        if (!SettingsManager.CurrentSettings.IsQuickLoaderUsed)
        {
            try
            {
                File.Copy(PathConstants.RA3QuickLoader, SettingsManager.CurrentSettings.GamePath, true);
                SettingsManager.CurrentSettings.IsQuickLoaderUsed = true;
                SettingsManager.SaveCurrentSettings();
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Error applying QuickLoader to the game", $"{ex.Message} {ex.InnerException}", TimeSpan.FromSeconds(5));
            }
        }
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        // Проверка, что нажата левая кнопка мыши
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (e.Source is Control hitElement && hitElement != this && IsInteractiveControl(hitElement))
            {
                // Если клик был по интерактивному элементу, не начинать перетаскивание
                return;
            }

            // Только если клик был по "фоновой" области окна, начинать перетаскивание
            BeginMoveDrag(e);
        }
    }

    // Вспомогательная функция для проверки, является ли элемент интерактивным
    private static bool IsInteractiveControl(Control control)
    {
        if (control == null)
        {
            return false;
        }

        // Список типов контролов, которые обычно обрабатывают клики сами
        // Можно расширить по необходимости
        Type[] interactiveTypes =
        [
            typeof(Button),
            typeof(ComboBox),
        ];

        Control? currentControl = control;
        while (currentControl != null)
        {
            if (interactiveTypes.Any(t => t.IsInstanceOfType(currentControl)))
            {
                return true;
            }
            // Поднимаемся по визуальному дереву
            currentControl = currentControl.Parent as Control ?? currentControl.TemplatedParent as Control;
        }

        return false;
    }
}
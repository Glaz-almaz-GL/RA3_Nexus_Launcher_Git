using Avalonia.Controls;
using Avalonia.Input;
using RA3_Nexus_Launcher.Helpers;
using RA3_Nexus_Launcher.Managers;
using System;
using System.Linq;

namespace RA3_Nexus_Launcher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DialogsManager.Initialize(this);
        GameProfilesHelper.CheckAndFixSkirmish();
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
        Type[] interactiveTypes = new[]
        {
            typeof(Button),
            typeof(ComboBox),
        };

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
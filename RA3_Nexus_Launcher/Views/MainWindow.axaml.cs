using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace RA3_Nexus_Launcher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        // Проверка, что нажата левая кнопка мыши (опционально)
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e); // Инициирует перетаскивание окна
        }
    }
}

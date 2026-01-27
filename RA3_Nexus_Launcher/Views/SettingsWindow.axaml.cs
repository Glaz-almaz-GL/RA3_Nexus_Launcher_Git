using Avalonia.Controls;
using Huskui.Avalonia.Controls;
using RA3_Nexus_Launcher.Managers;
using RA3_Nexus_Launcher.ViewModels;

namespace RA3_Nexus_Launcher.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        DataContext = new SettingsViewModel(this);
        InitializeComponent();
    }
}
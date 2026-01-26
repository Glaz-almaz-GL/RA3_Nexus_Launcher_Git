using Avalonia.Controls;
using RA3_Nexus_Launcher.ViewModels;

namespace RA3_Nexus_Launcher.Views;

public partial class SettingsView : Window
{
    public SettingsView()
    {
        DataContext = new SettingsViewModel();
        InitializeComponent();
    }
}
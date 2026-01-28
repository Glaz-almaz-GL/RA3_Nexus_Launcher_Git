using System;
using System.IO;

namespace RA3_Nexus_Launcher.Constants
{
    public static class PathConstants
    {
        public static readonly string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static readonly string DocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string RA3AppDataFolder = Path.Combine(AppDataFolder, "Red Alert 3");
        private static readonly string RA3DocumentsFolder = Path.Combine(DocumentsFolder, "Red Alert 3");

        public static readonly string LauncherSettingsFolder = Path.Combine(AppDataFolder, "RA3 Nexus Launcher");
        public static readonly string LauncherSettingsPath = Path.Combine(LauncherSettingsFolder, "settings.json");
        public static readonly string LauncherFolder = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string LauncherDataFolder = Path.Combine(LauncherFolder, "Data");
        public static readonly string LauncherRegistryFolder = Path.Combine(LauncherDataFolder, "Registry");
        public static readonly string LauncherToolsFolder = Path.Combine(LauncherDataFolder, "Tools");

        public static readonly string RA3ModFolder = Path.Combine(RA3DocumentsFolder, "Mods");
        public static readonly string RA3ReplayFolder = Path.Combine(RA3DocumentsFolder, "Replays");
        public static readonly string RA3MapsFolder = Path.Combine(RA3AppDataFolder, "Maps");
        public static readonly string RA3ProfilesFolder = Path.Combine(RA3AppDataFolder, "Profiles");

        public static readonly string RA3LaunchParametersTxt = Path.Combine(LauncherDataFolder, "LaunchParameters.txt");
        public static readonly string RA3RegistryFix64 = Path.Combine(LauncherRegistryFolder, "Fix_RA3_x64.reg");
        public static readonly string RA3RegistryFix32 = Path.Combine(LauncherRegistryFolder, "Fix_RA3_x32.reg");
        public static readonly string RA3FourGBPatch = Path.Combine(LauncherToolsFolder, "4GBPatch.exe");
        public static readonly string RA3QuickLoader = Path.Combine(LauncherToolsFolder, "RA3.exe");
        public static readonly string RA3OriginalLoader = Path.Combine(LauncherToolsFolder, "RA3-Original.exe");
    }
}

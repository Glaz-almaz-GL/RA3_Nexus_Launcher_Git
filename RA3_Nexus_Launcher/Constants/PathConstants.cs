using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RA3_Nexus_Launcher.Constants
{
    public static class PathConstants
    {
        public static readonly string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static readonly string DocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string RA3AppDataFolder = Path.Combine(AppDataFolder, "Red Alert 3");
        private static readonly string RA3DocumentsFolder = Path.Combine(DocumentsFolder, "Red Alert 3");

        public static readonly string LauncherFolder = Path.Combine(AppDataFolder, "RA3 Nexus Launcher");
        public static readonly string LauncherSettingsPath = Path.Combine(LauncherFolder, "settings.json");

        public static readonly string RA3ModFolder = Path.Combine(RA3DocumentsFolder, "Mods");
        public static readonly string RA3ReplayFolder = Path.Combine(RA3DocumentsFolder, "Replays");
        public static readonly string RA3MapsFolder = Path.Combine(RA3AppDataFolder, "Maps");
        public static readonly string RA3ProfilesFolder = Path.Combine(RA3AppDataFolder, "Profiles");
    }
}

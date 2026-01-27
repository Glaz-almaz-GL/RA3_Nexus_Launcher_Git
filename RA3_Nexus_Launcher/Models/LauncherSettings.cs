using RA3_Nexus_Launcher.Helpers;
using RA3_Nexus_Launcher.Models.Enums;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Text.Json.Serialization;

namespace RA3_Nexus_Launcher.Models
{
    public class LauncherSettings
    {
        [JsonPropertyName("GameFolderPath")] // Имя в JSON
        [AllowNull]
        public string GameFolder // Имя свойства
        {
            get => field ??= GameHelper.GetGameFolder();
            set;
        }

        [JsonIgnore]
        public string GamePath => Path.Combine(GameFolder, "RA3.exe");

        [JsonPropertyName("BattleNetPath")] // Имя в JSON
        [AllowNull]
        public string? BattleNetPath // Имя свойства
        {
            get => field ??= GameHelper.GetBattleNetPath();
            set;
        }

        [JsonPropertyName("LaunchParameters")]
        public string[]? LaunchParameters { get; set; } = null;

        [JsonPropertyName("Width")]
        public int Width { get; set; } = 1600;

        [JsonPropertyName("Height")]
        public int Height { get; set; } = 900;

        [JsonPropertyName("UseCustomResolution")]
        public bool UseCustomResolution { get; set; } = false;

        [JsonPropertyName("WindowParam")]
        public WindowParam WindowParam { get; set; } = WindowParam.FullScreen;

        [JsonPropertyName("RunVersion")]
        public string RunVersion { get; set; } = "1.12";

        [JsonPropertyName("InstalledLauncherVersion")]
        public Version InstalledVersion { get; set; } = new(1, 0, 0);

        public bool IsQuickLoaderUsed { get; set; } = false;

        // Конструктор по умолчанию
        public LauncherSettings()
        {
            GameFolder = GameHelper.GetGameFolder();
            BattleNetPath = GameHelper.GetBattleNetPath();
        }

        // Конструктор десериализации — имена параметров должны совпадать с именами СВОЙСТВ
#pragma warning disable S107
        [JsonConstructor]
        public LauncherSettings(
            string? gameFolder,
            string? battleNetPath,
            string[]? launchParameters,
            int width,
            int height,
            bool useCustomResolution,
            WindowParam windowParam,
            string runVersion,
            Version installedVersion)
        {
            // Присваиваем значения свойствам
            GameFolder = gameFolder;
            BattleNetPath = battleNetPath;
            LaunchParameters = launchParameters;
            Width = width;
            Height = height;
            UseCustomResolution = useCustomResolution;
            WindowParam = windowParam;
            RunVersion = runVersion;
            InstalledVersion = installedVersion;
        }
#pragma warning restore S107
    }
}
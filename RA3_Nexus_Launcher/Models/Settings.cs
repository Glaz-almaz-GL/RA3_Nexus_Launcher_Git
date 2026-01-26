using RA3_Nexus_Launcher.Helpers;
using RA3_Nexus_Launcher.Models.Enums;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json.Serialization;

namespace RA3_Nexus_Launcher.Models
{
    public class Settings
    {
        [JsonPropertyName("GameFolderPath")]
        [AllowNull]
        public string GameFolderPath
        {
            get => field ??= GameHelper.GetGameFolder();
            set;
        }

        public string GamePath => Path.Combine(GameFolderPath, "RA3.exe");

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

        // Конструктор по умолчанию - вызывается при создании нового экземпляра
        public Settings()
        {
            GameFolderPath = GameHelper.GetGameFolder();
        }

        // Этот конструктор будет вызываться при десериализации JSON
        [JsonConstructor]
        public Settings(string? gameFolderPath, string[]? launchParameters, int width, int height,
                       bool useCustomResolution, WindowParam windowParam, string runVersion)
        {
            GameFolderPath = gameFolderPath;
            LaunchParameters = launchParameters;
            Width = width;
            Height = height;
            UseCustomResolution = useCustomResolution;
            WindowParam = windowParam;
            RunVersion = runVersion;
        }
    }
}
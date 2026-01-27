using System.IO;

namespace RA3_Nexus_Launcher.Models
{
    public struct GameProfile(string name, string profilePath, string? ipAddress, string? gameSpyIdAddress)
    {
        public string Name { get; init; } = name;
        public string ProfilePath { get; init; } = profilePath;
        public readonly string OptionsPath => Path.Combine(ProfilePath, "Options.ini");
        public string? IPAddress { get; set; } = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress;
        public string? GameSpyIdAddress { get; set; } = string.IsNullOrWhiteSpace(gameSpyIdAddress) ? null : gameSpyIdAddress;

        // Audio Settings Group
        public AudioSettings AudioSettings { get; set; } = new AudioSettings();

        public override readonly string ToString()
        {
            return Name;
        }
    }

    // Nested struct to hold audio settings
    public struct AudioSettings
    {
        public AudioSettings()
        {
        }

        // Изменяем типы на nullable double для корректной записи в INI
        public double? AmbientVolume { get; set; } = 1.0;
        public double? MovieVolume { get; set; } = 1.0;
        public double? MusicVolume { get; set; } = 1.0;
        public double? SFXVolume { get; set; } = 1.0;
        public double? VoiceVolume { get; set; } = 1.0;
    }
}
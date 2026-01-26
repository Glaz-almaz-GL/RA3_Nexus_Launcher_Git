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

        public override readonly string ToString()
        {
            return Name;
        }
    }
}

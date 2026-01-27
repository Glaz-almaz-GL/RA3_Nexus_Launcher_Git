using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Helpers;
using RA3_Nexus_Launcher.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace RA3_Nexus_Launcher.Managers
{
    public static class GameProfilesManager
    {
        public static List<GameProfile> GetProfilesList()
        {
            string directoryIniPath = Path.Combine(PathConstants.RA3ProfilesFolder, "directory.ini");
            string? firstLine = EaDirectoryParser.ReadFirstLine(directoryIniPath);

            if (string.IsNullOrWhiteSpace(firstLine))
            {
                return [];
            }

            string original = EaDirectoryParser.ParseDirectoryString(firstLine);
            string[] directories = Directory.GetDirectories(PathConstants.RA3ProfilesFolder);
            List<GameProfile> profiles = [];

            foreach (string profilePath in directories)
            {
                string profileName = Path.GetFileNameWithoutExtension(profilePath);

                if (!string.IsNullOrWhiteSpace(profileName) && original.Contains(profileName))
                {
                    string optionsPath = Path.Combine(profilePath, "Options.ini");

                    (string? GameSpyIpAddress, string? IpAddress) = IniFileHelper.ReadIpAddresses(optionsPath);
                    (double? AmbientVol, double? MovieVol, double? MusicVol, double? SFXVol, double? VoiceVol) = IniFileHelper.ReadAudioVolumes(optionsPath);

                    AudioSettings audioSettings = new()
                    {
                        AmbientVolume = AmbientVol ?? 100.0,
                        MovieVolume = MovieVol ?? 100.0,
                        MusicVolume = MusicVol ?? 100.0,
                        SFXVolume = SFXVol ?? 100.0,
                        VoiceVolume = VoiceVol ?? 100.0
                    };

                    GameProfile profile = new(profileName, profilePath, IpAddress, GameSpyIpAddress)
                    {
                        AudioSettings = audioSettings
                    };

                    profiles.Add(profile);
                }
            }

            return profiles;
        }

        public static void UpdateProfileAudioVolumes(GameProfile profile)
        {
            IniFileHelper.WriteAudioVolumes(
                profile.OptionsPath,
                profile.AudioSettings.AmbientVolume,
                profile.AudioSettings.MovieVolume,
                profile.AudioSettings.MusicVolume,
                profile.AudioSettings.SFXVolume,
                profile.AudioSettings.VoiceVolume
            );
        }

        public static void UpdateProfileIpAddresses(GameProfile profile)
        {
            IniFileHelper.WriteIpAddresses(
                profile.OptionsPath,
                profile.GameSpyIdAddress,
                profile.IPAddress
            );
        }
    }
}
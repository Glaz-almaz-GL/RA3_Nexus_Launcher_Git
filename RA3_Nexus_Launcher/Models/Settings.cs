using RA3_Nexus_Launcher.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RA3_Nexus_Launcher.Models
{
    public class Settings
    {
        public string GameFolderPath { get; set; } = GameHelper.GetGamePath();
        public string[]? LaunchParameters { get; set; } = null;
        public string RunVersion { get; set; } = "1.12";
    }
}

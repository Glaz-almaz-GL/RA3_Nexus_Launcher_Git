using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RA3_Nexus_Launcher.Models
{
    public class InstalledModInfo
    {
        public string Name { get; set; } // DIVISION
        public string Version { get; set; } // 1.00
        public string ModPath { get; set; } // DIVISION_1.00.skudef
        public string NameAndVersion => $"{Name} v{Version}";

        public InstalledModInfo(string name, string version, string modPath)
        {
            Debug.WriteLine($"Name: {name}; Version: {version}; ModPath: {modPath}");

            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentException.ThrowIfNullOrEmpty(version);
            ArgumentException.ThrowIfNullOrEmpty(modPath);
            Name = name;
            Version = version;
            ModPath = modPath;
        }
    }
}

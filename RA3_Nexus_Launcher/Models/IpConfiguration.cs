using System;
using System.Collections.Generic;
using System.Text;

namespace RA3_Nexus_Launcher.Models
{
    public readonly struct IpConfiguration(string key, string? value, bool shouldBeRemoved)
    {
        public readonly string Key = key;
        public readonly string? Value = value;
        public readonly bool ShouldBeRemoved = shouldBeRemoved;

        public string GetValue()
        {
            return ShouldBeRemoved ? $"{Key} 0.0.0.0" : $"{Key} {Value}";
        }
    }
}

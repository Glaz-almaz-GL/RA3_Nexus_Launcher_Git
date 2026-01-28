using RA3_Nexus_Launcher.Constants;
using System;
using System.IO;
using System.Text;

namespace RA3_Nexus_Launcher.Helpers
{
    public static class SkirmishFixer
    {
        public static void CheckAndFixSkirmish()
        {
            try
            {
                if (!Directory.Exists(PathConstants.RA3ProfilesFolder))
                {
                    return;
                }

                foreach (string profileFolder in Directory.GetDirectories(PathConstants.RA3ProfilesFolder))
                {
                    string skirmishIniPath = Path.Combine(profileFolder, "Skirmish.ini");

                    if (!File.Exists(skirmishIniPath))
                    {
                        continue;
                    }

                    string[] data = File.ReadAllLines(skirmishIniPath);

                    if (data.Length == 0)
                    {
                        continue;
                    }

                    string[] splittedLine = data[0].Split(';');
                    if (splittedLine.Length < 2)
                    {
                        continue;
                    }

                    string playerCode = splittedLine[^2].Split(':')[0];

                    if (playerCode != "S=X")
                    {
                        continue;
                    }

                    string profileName = Path.GetFileName(profileFolder);
                    string fixedPlayerCode = $"S=H{profileName},0,0,TT,-1,7,-1,-1,0,1,-1,:X:X:X:X:X:";
                    splittedLine[^2] = fixedPlayerCode;

                    StringBuilder sb = new();
                    for (int i = 0; i < splittedLine.Length - 1; i++)
                    {
                        sb.Append(splittedLine[i]);
                        if (i < splittedLine.Length - 2)
                        {
                            sb.Append(';');
                        }
                    }

                    data[0] = sb.ToString();
                    File.WriteAllLines(skirmishIniPath, data);
                }
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Skirmish.ini file checking and fix error", $"{ex.Message} {ex.InnerException}", TimeSpan.FromSeconds(5));
            }
        }
    }
}
using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Helpers;
using RA3_Nexus_Launcher.Helpers.Patches;
using System;
using System.Diagnostics;
using System.IO;

namespace RA3_Nexus_Launcher.Managers
{
    public static class GamePatchesManager
    {
        public static string GenerateNewCDKey()
        {
            string cdKey = CDKeyHelper.GenerateCDKey();
            CDKeyHelper.ApplyCDKey(cdKey);
            return cdKey;
        }

        public static void InstallFourGBPatch()
        {
            FourGBPatchHelper.Install4GBPatch();
        }

        public static void FixRegistry()
        {
            RegistryFixHelper.FixRegistry();
        }

        public static void RestartWithAdministratorPrivileges()
        {
            ProcessStartInfo process = new()
            {
                FileName = Environment.ProcessPath!,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(process);
            Environment.Exit(0);
        }

        public static void ApplyQuickLoader()
        {
            try
            {
                File.Copy(PathConstants.RA3QuickLoader, SettingsManager.CurrentSettings.GamePath, true);
                SettingsManager.CurrentSettings.IsQuickLoaderUsed = true;
                SettingsManager.SaveCurrentSettings();

                NotificationHelpers.ShowSuccess("RA3 QuickLoader was successfully applied", $"The original RA3.exe is located at {PathConstants.RA3OriginalLoader}", TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                NotificationHelpers.ShowError("Error applying QuickLoader to the game", $"{ex.Message} {ex.InnerException}", TimeSpan.FromSeconds(5));
            }
        }
    }
}

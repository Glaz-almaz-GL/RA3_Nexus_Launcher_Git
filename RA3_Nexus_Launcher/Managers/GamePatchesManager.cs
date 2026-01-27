using RA3_Nexus_Launcher.Helpers.Patches;

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
    }
}

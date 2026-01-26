using RA3_Nexus_Launcher.Helpers.Patches;

namespace RA3_Nexus_Launcher.Managers
{
    public static class GamePatchesManager
    {
        public static void GenerateNewCDKey()
        {
            CDKeyHelper.ApplyCDKey(CDKeyHelper.GenerateCDKey());
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

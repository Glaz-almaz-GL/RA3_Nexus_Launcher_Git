namespace RA3_Nexus_Launcher.Helpers
{
    public static class StringHelper
    {
        public static string CapitalizeFirst(string input)
        {
            return string.IsNullOrEmpty(input) ? input : char.ToUpper(input[0]) + input[1..];
        }
    }
}

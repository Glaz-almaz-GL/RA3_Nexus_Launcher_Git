using RA3_Nexus_Launcher.Constants;
using RA3_Nexus_Launcher.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RA3_Nexus_Launcher.Managers
{
    public static partial class InstalledModsManager
    {
        public static List<InstalledModInfo> InstalledMods => GetModsFromDocuments();

        public static List<InstalledModInfo> GetModsFromDocuments()
        {
            List<InstalledModInfo> mods = [];

            if (Directory.Exists(PathConstants.RA3ModFolder))
            {
                foreach (string modPath in Directory.GetDirectories(PathConstants.RA3ModFolder))
                {
                    string[]? skudefFiles = Directory.GetFiles(modPath, "*.skudef");

                    if (skudefFiles?.Length > 0)
                    {
                        foreach (var skudef in skudefFiles.Where(skudef => !string.IsNullOrWhiteSpace(skudef)))
                        {
                            (string Name, string Version) = ParseFileName(Path.GetFileNameWithoutExtension(skudef));

                            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Version))
                            {
                                continue;
                            }

                            mods.Add(new(Name, Version, skudef));
                        }
                    }
                }
            }

            return mods;
        }

        private static (string Name, string Version) ParseFileName(string fileNameWithoutExtension)
        {
            // Регулярное выражение теперь ожидает X.Y или X.Y.Z и т.д.
            Regex regex = ModInfoRegex();
            Match match = regex.Match(fileNameWithoutExtension);

            if (match.Success)
            {
                string name = match.Groups[1].Value; // Часть до последнего подчёркивания и версии
                string version = match.Groups[2].Value; // Версия в формате X.Y или X.Y.Z и т.д.
                return (name, version);
            }

            // Если формат не подошёл, возвращаем пустые строки или выбрасываем исключение
            return (string.Empty, string.Empty);
        }

        // Паттерн: любые символы (1+), затем _, затем цифры.цифры (1+ раз, может повторяться)
        [GeneratedRegex(@"^(.+?)[ _]([0-9]+(?:\.[0-9]+)+)$", RegexOptions.IgnoreCase)]
        private static partial Regex ModInfoRegex();
    }
}

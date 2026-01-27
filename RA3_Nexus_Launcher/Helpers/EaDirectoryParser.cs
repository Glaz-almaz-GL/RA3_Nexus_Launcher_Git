using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RA3_Nexus_Launcher.Helpers
{
    public static class EaDirectoryParser
    {
        public static string ParseDirectoryString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            List<byte> bytes = [];

            int i = 0;
            while (i < input.Length)
            {
                char c = input[i];
                if (c != '_')
                {
                    bytes.Add(Convert.ToByte(c));
                    i++;
                }
                else
                {
                    if (i + 2 >= input.Length)
                    {
                        break;
                    }

                    string hexPair = input.Substring(i + 1, 2);
                    if (!byte.TryParse(hexPair, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte parsedByte))
                    {
                        throw new FormatException($"Invalid hexadecimal value: {hexPair}");
                    }

                    bytes.Add(parsedByte);
                    i += 3;
                }
            }

            return Encoding.Unicode.GetString([.. bytes]);
        }

        public static string? ReadFirstLine(string directoryIniPath)
        {
            if (!File.Exists(directoryIniPath))
            {
                return null;
            }

            try
            {
                using FileStream fileStream = new(directoryIniPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader reader = new(fileStream, Encoding.UTF8);
                return reader.ReadLine();
            }
            catch (IOException)
            {
                return null;
            }
        }
    }
}
using System;

namespace VTBL.AddressNormalizer.Console
{
    /// <summary>
    /// Форматированный вывод для демо-режима консоли.
    /// </summary>
    internal static class DemoConsoleWriter
    {
        public static void WriteTitle(string title)
        {
            System.Console.WriteLine();
            System.Console.WriteLine(new string('=', 72));
            System.Console.WriteLine(title);
            System.Console.WriteLine(new string('=', 72));
        }

        public static void WriteSection(string title)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("-- " + title + " " + new string('-', Math.Max(0, 66 - title.Length)));
        }

        public static void WriteSampleHeader(int index, int total)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("[" + index + "/" + total + "]");
        }

        public static void WriteField(string label, string value)
        {
            System.Console.WriteLine(label + ": " + (value ?? string.Empty));
        }

        public static void WriteHashPreview(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                WriteField("HASH", string.Empty);
                return;
            }

            var preview = hash.Length > 16 ? hash.Substring(0, 16) + "..." : hash;
            WriteField("HASH", preview);
        }

        public static void WriteUsage()
        {
            WriteTitle("VTBL.AddressNormalizer — demo");
            System.Console.WriteLine();
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("  VTBL.AddressNormalizer.Console                 все демо-секции");
            System.Console.WriteLine("  VTBL.AddressNormalizer.Console address         только BuildingAddress");
            System.Console.WriteLine("  VTBL.AddressNormalizer.Console unit            только BuildingUnit");
            System.Console.WriteLine("  VTBL.AddressNormalizer.Console address \"...\"   один произвольный адрес");
            System.Console.WriteLine("  VTBL.AddressNormalizer.Console unit \"...\"      одна произвольная строка new_flat");
            System.Console.WriteLine("  VTBL.AddressNormalizer.Console help            эта справка");
        }
    }
}

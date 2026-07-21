using System;

namespace VTBL.AddressNormalizer.Console
{
    /// <summary>
    /// Точка входа: демо парсинга BuildingAddress (строение) и BuildingUnit (помещения).
    /// </summary>
    class Program
    {
        /// <summary>
        /// Запускает демо по аргументам командной строки или полный прогон sample-наборов.
        /// </summary>
        /// <param name="args">
        /// Без аргументов — обе секции.
        /// <c>address</c> / <c>unit</c> — одна секция; опционально второй аргумент — произвольная строка.
        /// </param>
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                RunAll(null, null);
                return;
            }

            var mode = args[0].Trim();
            if (IsHelp(mode))
            {
                DemoConsoleWriter.WriteUsage();
                return;
            }

            var customInput = args.Length > 1 ? args[1] : null;

            if (IsAddressMode(mode))
            {
                BuildingAddressDemo.Run(customInput);
                return;
            }

            if (IsUnitMode(mode))
            {
                BuildingUnitDemo.Run(customInput);
                return;
            }

            DemoConsoleWriter.WriteUsage();
            System.Console.WriteLine();
            System.Console.WriteLine("Неизвестный режим: " + mode);
            Environment.ExitCode = 1;
        }

        private static void RunAll(string addressInput, string unitInput)
        {
            DemoConsoleWriter.WriteTitle("VTBL.AddressNormalizer — demo");
            System.Console.WriteLine("Нормализация адресов CRM: BuildingAddress + BuildingUnit");

            BuildingAddressDemo.Run(addressInput);
            BuildingUnitDemo.Run(unitInput);

            DemoConsoleWriter.WriteSection("Готово");
            System.Console.WriteLine("Подсказка: address | unit | help — см. справку.");
        }

        private static bool IsHelp(string mode)
        {
            return string.Equals(mode, "help", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mode, "-h", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mode, "--help", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mode, "/?", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAddressMode(string mode)
        {
            return string.Equals(mode, "address", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mode, "addr", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mode, "building", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUnitMode(string mode)
        {
            return string.Equals(mode, "unit", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mode, "flat", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mode, "indoor", StringComparison.OrdinalIgnoreCase);
        }
    }
}

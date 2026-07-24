using System.Collections.Generic;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    /// <summary>
    /// Реестр GapId известных контрактных пробелов BuildingUnit (слой E).
    /// Ожидаемые каноны хранятся в MemberData KnownGapTests, не здесь.
    /// </summary>
    internal static class BuildingUnitKnownGaps
    {
        public const string G01 = "G01";
        public const string G02 = "G02";
        public const string G03 = "G03";
        public const string G04 = "G04";
        public const string G05 = "G05";
        public const string G06 = "G06";

        /// <summary>
        /// Стабильный упорядоченный список GapId: G01…G06.
        /// </summary>
        public static readonly IReadOnlyList<string> Ids = new[]
        {
            G01,
            G02,
            G03,
            G04,
            G05,
            G06,
        };
    }
}

using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Infrastructure.Shared;
using System.Text.RegularExpressions;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingUnit
{
    /// <summary>
    /// Регулярные выражения классификатора <see cref="BuildingUnitClassifier"/>.
    /// </summary>
    public sealed partial class BuildingUnitClassifier
    {
        /// <summary>
        /// Мусор Excel: ошибка формулы в ячейке.
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>^#\S+</c></para>
        /// <list type="bullet">
        /// <item><description><c>^#</c> — строка начинается с «#».</description></item>
        /// <item><description><c>\S+</c> — далее один и более непробельных символов (код ошибки).</description></item>
        /// </list>
        /// <para>Примеры: «#ИМЯ?», «#Н/Д». Используется в <see cref="IsGarbage"/> → <see cref="BuildingUnitCategory.Garbage"/>.</para>
        /// </remarks>
        private static readonly Regex ExcelErrorRegex = new Regex(
            @"^#\S+",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Дата, ошибочно попавшая в поле квартиры/помещения.
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>^\d{1,2}\.\d{1,2}\.\d{4}$</c></para>
        /// <list type="bullet">
        /// <item><description><c>^\d{1,2}\.\d{1,2}\.\d{4}$</c> — вся строка — дата «дд.мм.гггг»
        /// (день и месяц 1–2 цифры, год — 4 цифры).</description></item>
        /// </list>
        /// <para>Примеры: «01.01.2020», «9.12.2023». Не матчит «14, помещ. 15».</para>
        /// </remarks>
        private static readonly Regex DateOnlyRegex = new Regex(
            @"^\d{1,2}\.\d{1,2}\.\d{4}$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // Alias на shared IndoorMarkerPatterns — минимальный diff в DetectMarkers.
        private static readonly Regex FloorMarkerRegex = IndoorMarkerPatterns.Floor;
        private static readonly Regex PremiseMarkerRegex = IndoorMarkerPatterns.Premise;
        private static readonly Regex RoomMarkerRegex = IndoorMarkerPatterns.Room;
        private static readonly Regex OfficeMarkerRegex = IndoorMarkerPatterns.Office;
        private static readonly Regex ApartmentMarkerRegex = IndoorMarkerPatterns.Apartment;
        private static readonly Regex CabinetMarkerRegex = IndoorMarkerPatterns.Cabinet;
        private static readonly Regex WorkplaceMarkerRegex = IndoorMarkerPatterns.Workplace;
        private static readonly Regex PartMarkerRegex = IndoorMarkerPatterns.Part;
        private static readonly Regex EntranceMarkerRegex = IndoorMarkerPatterns.Entrance;
        private static readonly Regex BlockMarkerRegex = IndoorMarkerPatterns.Block;
        private static readonly Regex SectionMarkerRegex = IndoorMarkerPatterns.Section;
        private static readonly Regex MailboxMarkerRegex = IndoorMarkerPatterns.Mailbox;
    }
}

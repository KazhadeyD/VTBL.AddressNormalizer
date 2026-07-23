using System.Collections.Generic;
using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.Shared
{
    /// <summary>
    /// Единый источник regex indoor-маркеров для BuildingUnitClassifier и BuildingAddress extract.
    /// </summary>
    internal static class IndoorMarkerPatterns
    {
        private static readonly RegexOptions MarkerOptions =
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;

        /// <summary>
        /// Маркер этажа и подземных уровней.
        /// </summary>
        public static Regex Floor { get; } = new Regex(
            @"(?<!\p{L})(?:ЭТАЖ|ЭТ(?!\p{L})|ПОДВАЛЬНЫЙ|ПОДВАЛ|ЦОКОЛЬНЫЙ|ЦОКОЛ)(?!\p{L})",
            MarkerOptions);

        /// <summary>
        /// Маркер помещения, в т.ч. нежилого.
        /// </summary>
        public static Regex Premise { get; } = new Regex(
            @"(?<!\p{L})(?:НЕЖ\.?\s*ПОМ|ПОМЕЩЕНИЯ|ПОМЕЩЕНИЕ|ПОМЕЩ|ПОМ)(?!\p{L})",
            MarkerOptions);

        /// <summary>
        /// Маркер комнаты.
        /// </summary>
        public static Regex Room { get; } = new Regex(
            @"(?<!\p{L})(?:КОМНАТА|КОМН|КОМ)(?!\p{L})|(?<!\p{L})К\.(?!\p{L})",
            MarkerOptions);

        /// <summary>
        /// Маркер офиса.
        /// </summary>
        public static Regex Office { get; } = new Regex(
            @"(?<!\p{L})(?:ОФИС|ОФ)(?!\p{L})",
            MarkerOptions);

        /// <summary>
        /// Маркер квартиры.
        /// </summary>
        public static Regex Apartment { get; } = new Regex(
            @"(?<!\p{L})(?:КВАРТИРА|КВ)(?!\p{L})",
            MarkerOptions);

        /// <summary>
        /// Маркер кабинета.
        /// </summary>
        public static Regex Cabinet { get; } = new Regex(
            @"(?<!\p{L})(?:КАБИНЕТ|КАБ)(?!\p{L})",
            MarkerOptions);

        /// <summary>
        /// Маркер рабочего места.
        /// </summary>
        public static Regex Workplace { get; } = new Regex(
            @"(?<!\p{L})РАБ\.?\s*М",
            MarkerOptions);

        /// <summary>
        /// Маркер части помещения.
        /// </summary>
        public static Regex Part { get; } = new Regex(
            @"(?<!\p{L})Ч\.?\s*П",
            MarkerOptions);

        /// <summary>
        /// Маркер подъезда.
        /// </summary>
        public static Regex Entrance { get; } = new Regex(
            @"(?<!\p{L})ПОДЪЕЗД(?!\p{L})",
            MarkerOptions);

        /// <summary>
        /// Маркер блока.
        /// </summary>
        public static Regex Block { get; } = new Regex(
            @"(?<!\p{L})БЛОК(?!\p{L})",
            MarkerOptions);

        /// <summary>
        /// Маркер секции.
        /// </summary>
        public static Regex Section { get; } = new Regex(
            @"(?<!\p{L})СЕКЦ",
            MarkerOptions);

        /// <summary>
        /// Маркер абонентского ящика.
        /// </summary>
        public static Regex Mailbox { get; } = new Regex(
            @"(?<!\p{L})А/Я(?!\p{L})",
            MarkerOptions);

        /// <summary>
        /// Все indoor-маркеры в фиксированном порядке (12 шт.).
        /// </summary>
        public static IReadOnlyList<IndoorMarkerPatternDefinition> All { get; } = new[]
        {
            new IndoorMarkerPatternDefinition(Apartment, IndoorMarkerKind.Apartment),
            new IndoorMarkerPatternDefinition(Office, IndoorMarkerKind.Office),
            new IndoorMarkerPatternDefinition(Premise, IndoorMarkerKind.Premise),
            new IndoorMarkerPatternDefinition(Room, IndoorMarkerKind.Room),
            new IndoorMarkerPatternDefinition(Cabinet, IndoorMarkerKind.Cabinet),
            new IndoorMarkerPatternDefinition(Floor, IndoorMarkerKind.Floor),
            new IndoorMarkerPatternDefinition(Entrance, IndoorMarkerKind.Entrance),
            new IndoorMarkerPatternDefinition(Block, IndoorMarkerKind.Block),
            new IndoorMarkerPatternDefinition(Section, IndoorMarkerKind.Section),
            new IndoorMarkerPatternDefinition(Workplace, IndoorMarkerKind.Workplace),
            new IndoorMarkerPatternDefinition(Part, IndoorMarkerKind.Part),
            new IndoorMarkerPatternDefinition(Mailbox, IndoorMarkerKind.Mailbox),
        };
    }
}

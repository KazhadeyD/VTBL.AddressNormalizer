using System.Collections.Generic;
using System.Text.RegularExpressions;
using VTBL.AddressNormalizer.Abstractions.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.Shared
{
    /// <summary>
    /// Единый источник regex indoor-маркеров для BuildingUnit parser и BuildingAddress extract.
    /// </summary>
    /// <remarks>
    /// Лексемы — <see cref="IndoorMarkerLexemes"/>; сборка — <see cref="IndoorMarkerRegexFactory"/>.
    /// </remarks>
    internal static class IndoorMarkerPatterns
    {
        /// <summary>
        /// Маркер этажа и подземных уровней.
        /// </summary>
        public static Regex Floor { get; } =
            IndoorMarkerRegexFactory.MarkerOnly(IndoorMarkerLexemes.Floor);

        /// <summary>
        /// Маркер помещения, в т.ч. нежилого.
        /// </summary>
        public static Regex Premise { get; } =
            IndoorMarkerRegexFactory.MarkerOnly(IndoorMarkerLexemes.Premise);

        /// <summary>
        /// Маркер комнаты.
        /// </summary>
        public static Regex Room { get; } = new Regex(
            $@"(?<!\p{{L}})(?:{IndoorMarkerLexemes.Room})(?!\p{{L}})|(?<!\p{{L}}){IndoorMarkerLexemes.ShortRoom}(?!\p{{L}})",
            IndoorMarkerRegexFactory.MarkerOptions);

        /// <summary>
        /// Маркер офиса.
        /// </summary>
        public static Regex Office { get; } =
            IndoorMarkerRegexFactory.MarkerOnly(IndoorMarkerLexemes.Office);

        /// <summary>
        /// Маркер квартиры.
        /// </summary>
        public static Regex Apartment { get; } =
            IndoorMarkerRegexFactory.MarkerOnly(IndoorMarkerLexemes.Apartment);

        /// <summary>
        /// Маркер кабинета.
        /// </summary>
        public static Regex Cabinet { get; } =
            IndoorMarkerRegexFactory.MarkerOnly(IndoorMarkerLexemes.Cabinet);

        /// <summary>
        /// Маркер рабочего места.
        /// </summary>
        public static Regex Workplace { get; } =
            IndoorMarkerRegexFactory.MarkerPrefix(IndoorMarkerLexemes.Workplace);

        /// <summary>
        /// Маркер части помещения.
        /// </summary>
        public static Regex Part { get; } =
            IndoorMarkerRegexFactory.MarkerPrefix(IndoorMarkerLexemes.Part);

        /// <summary>
        /// Маркер подъезда.
        /// </summary>
        public static Regex Entrance { get; } =
            IndoorMarkerRegexFactory.MarkerOnly(IndoorMarkerLexemes.Entrance);

        /// <summary>
        /// Маркер проезда («проезд», «пр-д»; опционально ведущий порядковый «1-й»).
        /// </summary>
        public static Regex Passage { get; } = new Regex(
            $@"(?<!\p{{L}})(?:\d+\s*-\s*[ЙЯ]\s+)?(?:{IndoorMarkerLexemes.Passage})(?!\p{{L}})",
            IndoorMarkerRegexFactory.MarkerOptions);

        /// <summary>
        /// Маркер владения («владение», «влад», «вл.»).
        /// </summary>
        public static Regex Holding { get; } =
            IndoorMarkerRegexFactory.MarkerOnly(IndoorMarkerLexemes.Holding + @"\.?");

        /// <summary>
        /// Маркер склада («склад», «скл.»).
        /// </summary>
        public static Regex Storage { get; } =
            IndoorMarkerRegexFactory.MarkerOnly(IndoorMarkerLexemes.Storage + @"\.?");

        /// <summary>
        /// Маркер блока.
        /// </summary>
        public static Regex Block { get; } =
            IndoorMarkerRegexFactory.MarkerOnly(IndoorMarkerLexemes.Block);

        /// <summary>
        /// Маркер секции.
        /// </summary>
        public static Regex Section { get; } =
            IndoorMarkerRegexFactory.MarkerPrefix(IndoorMarkerLexemes.Section);

        /// <summary>
        /// Маркер абонентского ящика.
        /// </summary>
        public static Regex Mailbox { get; } =
            IndoorMarkerRegexFactory.MarkerOnly(IndoorMarkerLexemes.Mailbox);

        /// <summary>
        /// Все indoor-маркеры в фиксированном порядке (15 шт.).
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
            new IndoorMarkerPatternDefinition(Passage, IndoorMarkerKind.Passage),
            new IndoorMarkerPatternDefinition(Holding, IndoorMarkerKind.Holding),
            new IndoorMarkerPatternDefinition(Storage, IndoorMarkerKind.Storage),
            new IndoorMarkerPatternDefinition(Block, IndoorMarkerKind.Block),
            new IndoorMarkerPatternDefinition(Section, IndoorMarkerKind.Section),
            new IndoorMarkerPatternDefinition(Workplace, IndoorMarkerKind.Workplace),
            new IndoorMarkerPatternDefinition(Part, IndoorMarkerKind.Part),
            new IndoorMarkerPatternDefinition(Mailbox, IndoorMarkerKind.Mailbox),
        };
    }
}

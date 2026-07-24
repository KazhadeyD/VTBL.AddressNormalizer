namespace VTBL.AddressNormalizer.Infrastructure.Shared
{
    /// <summary>
    /// Общие альтернативы indoor-маркеров для extract (<see cref="IndoorMarkerPatterns"/>)
    /// и value-capturing regex парсера (<see cref="BuildingUnit.BuildingUnitParser"/>).
    /// </summary>
    /// <remarks>
    /// Только тело <c>(?:…)</c> без границ слова и без группы значения —
    /// границы/захват собирает <see cref="IndoorMarkerRegexFactory"/>.
    /// </remarks>
    internal static class IndoorMarkerLexemes
    {
        public const string Apartment = @"КВАРТИРА|КВ";
        public const string Cabinet = @"КАБИНЕТ|КАБ";
        public const string Office = @"ОФИС|ОФ";
        public const string Entrance = @"ПОДЪЕЗД";
        public const string Passage = @"ПРОЕЗД|ПР-Д";

        /// <summary>
        /// Владение без опциональной точки после маркера (точку добавляет фабрика).
        /// </summary>
        public const string Holding = @"ВЛАДЕНИЕ|ВЛАД(?!\p{L})|ВЛ";

        /// <summary>
        /// Склад без опциональной точки после маркера.
        /// </summary>
        public const string Storage = @"СКЛАД|СКЛ";

        public const string Block = @"БЛОК";
        public const string Section = @"СЕКЦ";
        public const string SectionFull = @"СЕКЦИЯ";
        public const string Mailbox = @"А/Я";
        public const string Litera = @"ЛИТЕ?РА?";

        public const string Floor = @"ЭТАЖ|ЭТ(?!\p{L})|ПОДВАЛЬНЫЙ|ПОДВАЛ|ЦОКОЛЬНЫЙ|ЦОКОЛ";
        public const string Premise = @"НЕЖ\.?\s*ПОМ|ПОМЕЩЕНИЯ|ПОМЕЩЕНИЕ|ПОМЕЩ|ПОМ";
        public const string Room = @"КОМНАТА|КОМН|КОМ";
        public const string ShortRoom = @"К\.";
        public const string Workplace = @"РАБ\.?\s*М";
        public const string Part = @"Ч\.?\s*П";
    }
}

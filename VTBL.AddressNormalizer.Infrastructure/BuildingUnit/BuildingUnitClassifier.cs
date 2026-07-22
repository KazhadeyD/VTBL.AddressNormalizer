using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using System.Text.RegularExpressions;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingUnit
{
    /// <summary>
    /// Классификация сырой indoor/unit-строки перед разбором.
    /// </summary>
    public sealed partial class BuildingUnitClassifier : IBuildingUnitClassifier
    {
        /// <summary>
        /// Набор флагов наличия маркеров типов локации в строке.
        /// </summary>
        private readonly struct MarkerFlags
        {
            /// <summary>
            /// Создаёт флаги по результатам детекции маркеров.
            /// </summary>
            public MarkerFlags(
                bool floor,
                bool premise,
                bool room,
                bool office,
                bool apartment,
                bool cabinet,
                bool workplace,
                bool part,
                bool entrance,
                bool block,
                bool section,
                bool mailbox)
            {
                Floor = floor;
                Premise = premise;
                Room = room;
                Office = office;
                Apartment = apartment;
                Cabinet = cabinet;
                Workplace = workplace;
                Part = part;
                Entrance = entrance;
                Block = block;
                Section = section;
                Mailbox = mailbox;
            }

            /// <summary>Маркер этажа (ЭТ, ЭТАЖ, подвал, цоколь).</summary>
            public bool Floor { get; }
            /// <summary>Маркер помещения (ПОМ, ПОМЕЩ., НЕЖ.ПОМ).</summary>
            public bool Premise { get; }
            /// <summary>Маркер комнаты (КОМ, КОМН., К.).</summary>
            public bool Room { get; }
            /// <summary>Маркер офиса (ОФ, ОФИС).</summary>
            public bool Office { get; }
            /// <summary>Маркер квартиры (КВ, КВАРТИРА).</summary>
            public bool Apartment { get; }
            /// <summary>Маркер кабинета (КАБ, КАБИНЕТ).</summary>
            public bool Cabinet { get; }
            /// <summary>Маркер рабочего места (РАБ.М.).</summary>
            public bool Workplace { get; }
            /// <summary>Маркер части помещения (Ч.П.).</summary>
            public bool Part { get; }
            /// <summary>Маркер подъезда (ПОДЪЕЗД).</summary>
            public bool Entrance { get; }
            /// <summary>Маркер блока (БЛОК).</summary>
            public bool Block { get; }
            /// <summary>Маркер секции (СЕКЦ…).</summary>
            public bool Section { get; }
            /// <summary>Маркер абонентского ящика (А/Я).</summary>
            public bool Mailbox { get; }

            /// <summary>
            /// Число различных типов локации, обнаруженных в строке.
            /// </summary>
            public int CategoryCount
            {
                get
                {
                    var count = 0;
                    if (Floor) count++;
                    if (Premise) count++;
                    if (Room) count++;
                    if (Office) count++;
                    if (Apartment) count++;
                    if (Cabinet) count++;
                    if (Workplace) count++;
                    if (Part) count++;
                    if (Entrance) count++;
                    if (Block) count++;
                    if (Section) count++;
                    if (Mailbox) count++;
                    return count;
                }
            }

            /// <summary>
            /// Есть ли хотя бы один маркер типизированной локации (эт/пом/ком/оф/кв/каб/раб.м/ч.п).
            /// </summary>
            public bool HasLocationMarker =>
                Floor || Premise || Room || Office || Apartment || Cabinet || Workplace || Part;

            /// <summary>
            /// Только служебные маркеры (а/я, подъезд) без типизированной локации и без блок/секция.
            /// </summary>
            public bool HasServiceMarkerOnly =>
                (Mailbox || Entrance) && !HasLocationMarker && !Block && !Section;
        }

        /// <summary>
        /// Определяет категорию строки по маркерам и правилам приоритета.
        /// </summary>
        /// <param name="input">Сырая строка локации внутри здания.</param>
        /// <returns>Доминирующая категория по маркерам; без маркеров — <see cref="BuildingUnitCategory.Unknown"/>.</returns>
        public BuildingUnitCategory Classify(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return BuildingUnitCategory.Unknown;

            var text = Preprocess(input);
            if (IsGarbage(text))
                return BuildingUnitCategory.Garbage;

            var markers = DetectMarkers(text);

            if (markers.HasServiceMarkerOnly)
                return BuildingUnitCategory.ServiceMarker;

            if (markers.Apartment && !markers.Premise && !markers.Room && !markers.Office &&
                !markers.Floor && !markers.Cabinet)
                return BuildingUnitCategory.Apartment;

            if (markers.Cabinet && !markers.Premise && !markers.Room && !markers.Office && !markers.Floor)
                return BuildingUnitCategory.Cabinet;

            if (markers.CategoryCount >= 3)
                return BuildingUnitCategory.Mixed;

            if (markers.Premise)
                return BuildingUnitCategory.Premise;

            if (markers.Office)
                return BuildingUnitCategory.Office;

            if (markers.Room)
                return BuildingUnitCategory.Room;

            if (markers.Floor)
                return BuildingUnitCategory.Floor;

            return BuildingUnitCategory.Unknown;
        }

        /// <summary>
        /// Препроцессинг для классификатора: trim, снятие кавычек, схлопывание пробелов.
        /// </summary>
        private static string Preprocess(string input)
        {
            var text = input.Trim();
            if (text.Length >= 2 && text[0] == '"' && text[text.Length - 1] == '"')
                text = text.Substring(1, text.Length - 2).Trim();

            text = Regex.Replace(text, @"\s+", " ", RegexOptions.CultureInvariant).Trim();
            return text;
        }

        /// <summary>
        /// Определяет, является ли строка мусором источника (Excel-ошибка, дата).
        /// </summary>
        private static bool IsGarbage(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            if (ExcelErrorRegex.IsMatch(text))
                return true;

            if (DateOnlyRegex.IsMatch(text))
                return true;

            return false;
        }

        /// <summary>
        /// Детектирует наличие маркеров типов локации в препроцессированной строке.
        /// </summary>
        private static MarkerFlags DetectMarkers(string text)
        {
            return new MarkerFlags(
                FloorMarkerRegex.IsMatch(text),
                PremiseMarkerRegex.IsMatch(text),
                RoomMarkerRegex.IsMatch(text),
                OfficeMarkerRegex.IsMatch(text),
                ApartmentMarkerRegex.IsMatch(text),
                CabinetMarkerRegex.IsMatch(text),
                WorkplaceMarkerRegex.IsMatch(text),
                PartMarkerRegex.IsMatch(text),
                EntranceMarkerRegex.IsMatch(text),
                BlockMarkerRegex.IsMatch(text),
                SectionMarkerRegex.IsMatch(text),
                MailboxMarkerRegex.IsMatch(text));
        }
    }
}

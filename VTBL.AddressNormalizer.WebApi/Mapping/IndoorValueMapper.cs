using System;
using System.Collections.Generic;
using System.Linq;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.WebApi.Models;

namespace VTBL.AddressNormalizer.WebApi.Mapping
{
    /// <summary>
    /// Маппинг <see cref="BuildingUnitLocation"/> → typed <see cref="IndoorValueDto"/> (вариант B).
    /// </summary>
    public static class IndoorValueMapper
    {
        /// <summary>Русские имена категорий (ТЗ 2.A) — единый источник констант.</summary>
        public static class CategoryNames
        {
            public const string Floors = "этаж";
            public const string Premises = "помещение";
            public const string Rooms = "комната";
            public const string Offices = "офис";
            public const string Workplaces = "рабочее место";
            public const string Parts = "часть помещения";
            public const string Apartments = "квартира";
            public const string Cabinets = "кабинет";
            public const string Entrances = "подъезд";
            public const string Blocks = "блок";
            public const string Sections = "секция";
            public const string Mailboxes = "а/я";
            public const string Literas = "литера";
            public const string Ranges = "диапазон";
            public const string RawCodes = "код";
            public const string Notes = "примечание";
            public const string Unparsed = "неразобранное";
        }

        /// <summary>
        /// Строит полный <see cref="IndoorValueDto"/> со всеми 17 категориями (вариант B).
        /// Пустые коллекции Location → <c>values: []</c>; свойства никогда не null.
        /// </summary>
        /// <param name="location">Локация unit; <c>null</c> трактуется как пустая локация.</param>
        public static IndoorValueDto ToVariantB(BuildingUnitLocation location)
        {
            var src = location ?? new BuildingUnitLocation();

            return new IndoorValueDto
            {
                Floors = Category(CategoryNames.Floors, src.Floors),
                Premises = Category(CategoryNames.Premises, src.Premises),
                Rooms = Category(CategoryNames.Rooms, src.Rooms),
                Offices = Category(CategoryNames.Offices, src.Offices),
                Workplaces = Category(CategoryNames.Workplaces, src.Workplaces),
                Parts = Category(CategoryNames.Parts, src.Parts),
                Apartments = Category(CategoryNames.Apartments, src.Apartments),
                Cabinets = Category(CategoryNames.Cabinets, src.Cabinets),
                Entrances = Category(CategoryNames.Entrances, src.Entrances),
                Blocks = Category(CategoryNames.Blocks, src.Blocks),
                Sections = Category(CategoryNames.Sections, src.Sections),
                Mailboxes = Category(CategoryNames.Mailboxes, src.Mailboxes),
                Literas = Category(CategoryNames.Literas, src.Literas),
                Ranges = Category(CategoryNames.Ranges, src.Ranges),
                RawCodes = Category(CategoryNames.RawCodes, src.RawCodes),
                Notes = Category(CategoryNames.Notes, src.Notes),
                Unparsed = Category(CategoryNames.Unparsed, src.Unparsed)
            };
        }

        private static IndoorCategoryDto Category(string name, IList<string> values) =>
            new IndoorCategoryDto
            {
                Name = name,
                Values = CopyValues(values)
            };

        private static string[] CopyValues(IList<string> values)
        {
            if (values == null || values.Count == 0)
                return Array.Empty<string>();

            return values.ToArray();
        }
    }
}

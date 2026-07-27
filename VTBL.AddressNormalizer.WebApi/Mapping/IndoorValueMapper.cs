using System;
using System.Collections.Generic;
using System.Linq;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.WebApi.Models;

namespace VTBL.AddressNormalizer.WebApi.Mapping
{
    /// <summary>
    /// Маппинг <see cref="BuildingUnitLocation"/> → <see cref="IndoorValueDto"/>
    /// (sparse <c>marks</c> с id, русским <c>name</c> и <c>values</c>).
    /// </summary>
    public static class IndoorValueMapper
    {
        /// <summary>
        /// Стабильные id категорий для JSON <c>marks[].id</c>.
        /// </summary>
        public static class MarkIds
        {
            public const string Floor = "floor";
            public const string Premise = "premise";
            public const string Room = "room";
            public const string Office = "office";
            public const string Workplace = "workplace";
            public const string Part = "part";
            public const string Apartment = "apartment";
            public const string Cabinet = "cabinet";
            public const string Entrance = "entrance";
            public const string Passage = "passage";
            public const string Holding = "holding";
            public const string Storage = "storage";
            public const string Block = "block";
            public const string Section = "section";
            public const string Mailbox = "mailbox";
            public const string Litera = "litera";
            public const string Range = "range";
            public const string RawCode = "rawCode";
            public const string Note = "note";
            public const string Unparsed = "unparsed";
        }

        /// <summary>
        /// Русские отображаемые имена категорий — единый источник констант.
        /// </summary>
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
            public const string Passages = "проезд";
            public const string Holdings = "владение";
            public const string Storages = "склад";
            public const string Blocks = "блок";
            public const string Sections = "секция";
            public const string Mailboxes = "а/я";
            public const string Literas = "литера";
            public const string Ranges = "диапазон";
            public const string RawCodes = "код";
            public const string Notes = "примечание";
            public const string Unparsed = "неразобранное";
        }

        private static readonly (string Id, string Name, Func<BuildingUnitLocation, IList<string>> GetValues)[] Catalog =
        {
            (MarkIds.Floor, CategoryNames.Floors, location => location.Floors),
            (MarkIds.Premise, CategoryNames.Premises, location => location.Premises),
            (MarkIds.Room, CategoryNames.Rooms, location => location.Rooms),
            (MarkIds.Office, CategoryNames.Offices, location => location.Offices),
            (MarkIds.Workplace, CategoryNames.Workplaces, location => location.Workplaces),
            (MarkIds.Part, CategoryNames.Parts, location => location.Parts),
            (MarkIds.Apartment, CategoryNames.Apartments, location => location.Apartments),
            (MarkIds.Cabinet, CategoryNames.Cabinets, location => location.Cabinets),
            (MarkIds.Entrance, CategoryNames.Entrances, location => location.Entrances),
            (MarkIds.Passage, CategoryNames.Passages, location => location.Passages),
            (MarkIds.Holding, CategoryNames.Holdings, location => location.Holdings),
            (MarkIds.Storage, CategoryNames.Storages, location => location.Storages),
            (MarkIds.Block, CategoryNames.Blocks, location => location.Blocks),
            (MarkIds.Section, CategoryNames.Sections, location => location.Sections),
            (MarkIds.Mailbox, CategoryNames.Mailboxes, location => location.Mailboxes),
            (MarkIds.Litera, CategoryNames.Literas, location => location.Literas),
            (MarkIds.Range, CategoryNames.Ranges, location => location.Ranges),
            (MarkIds.RawCode, CategoryNames.RawCodes, location => location.RawCodes),
            (MarkIds.Note, CategoryNames.Notes, location => location.Notes),
            (MarkIds.Unparsed, CategoryNames.Unparsed, location => location.Unparsed)
        };

        /// <summary>
        /// Строит <see cref="IndoorValueDto"/> с sparse <c>marks</c> — только категории с данными.
        /// </summary>
        /// <param name="location">Локация unit; <c>null</c> трактуется как пустая локация.</param>
        public static IndoorValueDto ToIndoorValueDto(BuildingUnitLocation location)
        {
            var src = location ?? new BuildingUnitLocation();
            var marks = new List<IndoorMarkDto>(Catalog.Length);

            foreach (var (id, name, getValues) in Catalog)
            {
                var values = CopyValues(getValues(src));
                if (values.Length == 0)
                    continue;

                marks.Add(new IndoorMarkDto
                {
                    Id = id,
                    Name = name,
                    Values = values
                });
            }

            return new IndoorValueDto
            {
                Marks = marks
            };
        }

        private static string[] CopyValues(IList<string> values)
        {
            if (values == null || values.Count == 0)
                return Array.Empty<string>();

            return values.ToArray();
        }
    }
}

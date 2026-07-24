namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Structured <c>indoorValue</c>: все категории <c>BuildingUnitLocation</c> с русским name и values,
    /// плюс <c>hash</c> от unit-канона.
    /// Свойства категорий никогда не null — пустые имеют <c>values: []</c>.
    /// </summary>
    public class IndoorValueDto
    {
        /// <summary>
        /// SHA256 (hex, lowercase) от канонической строки unit (<c>ToCanonical</c>).
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Этаж (<c>name</c>: «этаж»).
        /// </summary>
        public IndoorCategoryDto Floors { get; set; } = CreateEmpty();

        /// <summary>
        /// Помещение (<c>name</c>: «помещение»).
        /// </summary>
        public IndoorCategoryDto Premises { get; set; } = CreateEmpty();

        /// <summary>
        /// Комната (<c>name</c>: «комната»).
        /// </summary>
        public IndoorCategoryDto Rooms { get; set; } = CreateEmpty();

        /// <summary>
        /// Офис (<c>name</c>: «офис»).
        /// </summary>
        public IndoorCategoryDto Offices { get; set; } = CreateEmpty();

        /// <summary>
        /// Рабочее место (<c>name</c>: «рабочее место»).
        /// </summary>
        public IndoorCategoryDto Workplaces { get; set; } = CreateEmpty();

        /// <summary>
        /// Часть помещения (<c>name</c>: «часть помещения»).
        /// </summary>
        public IndoorCategoryDto Parts { get; set; } = CreateEmpty();

        /// <summary>
        /// Квартира (<c>name</c>: «квартира»).
        /// </summary>
        public IndoorCategoryDto Apartments { get; set; } = CreateEmpty();

        /// <summary>
        /// Кабинет (<c>name</c>: «кабинет»).
        /// </summary>
        public IndoorCategoryDto Cabinets { get; set; } = CreateEmpty();

        /// <summary>
        /// Подъезд (<c>name</c>: «подъезд»).
        /// </summary>
        public IndoorCategoryDto Entrances { get; set; } = CreateEmpty();

        /// <summary>
        /// Проезд (<c>name</c>: «проезд»).
        /// </summary>
        public IndoorCategoryDto Passages { get; set; } = CreateEmpty();

        /// <summary>
        /// Владение (<c>name</c>: «владение»).
        /// </summary>
        public IndoorCategoryDto Holdings { get; set; } = CreateEmpty();

        /// <summary>
        /// Склад (<c>name</c>: «склад»).
        /// </summary>
        public IndoorCategoryDto Storages { get; set; } = CreateEmpty();

        /// <summary>
        /// Блок (<c>name</c>: «блок»).
        /// </summary>
        public IndoorCategoryDto Blocks { get; set; } = CreateEmpty();

        /// <summary>
        /// Секция (<c>name</c>: «секция»).
        /// </summary>
        public IndoorCategoryDto Sections { get; set; } = CreateEmpty();

        /// <summary>
        /// Абонентский ящик (<c>name</c>: «а/я»).
        /// </summary>
        public IndoorCategoryDto Mailboxes { get; set; } = CreateEmpty();

        /// <summary>
        /// Литера (<c>name</c>: «литера»).
        /// </summary>
        public IndoorCategoryDto Literas { get; set; } = CreateEmpty();

        /// <summary>
        /// Диапазон номеров (<c>name</c>: «диапазон»).
        /// </summary>
        public IndoorCategoryDto Ranges { get; set; } = CreateEmpty();

        /// <summary>
        /// Код без маркера типа (<c>name</c>: «код»).
        /// </summary>
        public IndoorCategoryDto RawCodes { get; set; } = CreateEmpty();

        /// <summary>
        /// Примечание (<c>name</c>: «примечание»).
        /// </summary>
        public IndoorCategoryDto Notes { get; set; } = CreateEmpty();

        /// <summary>
        /// Неразобранные фрагменты (<c>name</c>: «неразобранное»).
        /// </summary>
        public IndoorCategoryDto Unparsed { get; set; } = CreateEmpty();

        private static IndoorCategoryDto CreateEmpty() =>
            new IndoorCategoryDto { Name = string.Empty, Values = System.Array.Empty<string>() };
    }
}

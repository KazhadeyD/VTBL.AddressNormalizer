using System.Collections.Generic;

namespace VTBL.AddressNormalizer.Abstractions.BuildingUnit
{
    /// <summary>
    /// Структурированное представление локации внутри здания.
    /// </summary>
    /// <remarks>
    /// Поля заполняются парсером; канонические префиксы — в канонизаторе.
    /// Наследует семантику базового разбора эт/пом/ком/оф и добавляет flat-специфичные сегменты.
    /// </remarks>
    public sealed class BuildingUnitLocation
    {
        /// <summary>
        /// Этаж («ЭТ», «ЭТАЖ», slash-цепочки).
        /// Канон: <c>эт:</c>.
        /// </summary>
        public IList<string> Floors { get; } = new List<string>();

        /// <summary>
        /// Помещение («ПОМ», «ПОМЕЩ.», «ПОМЕЩЕНИЕ»).
        /// Канон: <c>пом:</c>.
        /// </summary>
        public IList<string> Premises { get; } = new List<string>();

        /// <summary>
        /// Комната («КОМ», «КОМН.», «КОМНАТА», «К.»).
        /// Канон: <c>ком:</c>.
        /// </summary>
        public IList<string> Rooms { get; } = new List<string>();

        /// <summary>
        /// Офис («ОФ», «ОФИС»).
        /// Канон: <c>оф:</c>.
        /// </summary>
        public IList<string> Offices { get; } = new List<string>();

        /// <summary>
        /// Рабочее место («РАБ.М.», «РАБ М»).
        /// Канон: <c>раб.м:</c>.
        /// </summary>
        public IList<string> Workplaces { get; } = new List<string>();

        /// <summary>
        /// Часть помещения («Ч.П.», «Ч П»).
        /// Канон: <c>ч.п:</c>.
        /// </summary>
        public IList<string> Parts { get; } = new List<string>();

        /// <summary>
        /// Квартира («КВ», «КВАРТИРА»).
        /// Канон: <c>кв:</c>.
        /// </summary>
        public IList<string> Apartments { get; } = new List<string>();

        /// <summary>
        /// Кабинет («КАБ», «КАБИНЕТ»).
        /// Канон: <c>каб:</c>.
        /// </summary>
        public IList<string> Cabinets { get; } = new List<string>();

        /// <summary>
        /// Подъезд («ПОДЪЕЗД»).
        /// Канон: <c>под:</c>.
        /// </summary>
        public IList<string> Entrances { get; } = new List<string>();

        /// <summary>
        /// Проезд («ПРОЕЗД», «ПР-Д»; также «1-й проезд»).
        /// Канон: <c>проезд:</c>.
        /// </summary>
        public IList<string> Passages { get; } = new List<string>();

        /// <summary>
        /// Владение («ВЛАДЕНИЕ», «ВЛАД», «ВЛ.»).
        /// Канон: <c>влад:</c>.
        /// </summary>
        public IList<string> Holdings { get; } = new List<string>();

        /// <summary>
        /// Блок («БЛОК»; для «БЛОК-СЕКЦИЯ» дублируется в <see cref="Sections"/>).
        /// Канон: <c>блок:</c>.
        /// </summary>
        public IList<string> Blocks { get; } = new List<string>();

        /// <summary>
        /// Секция («СЕКЦИЯ»; для «БЛОК-СЕКЦИЯ» дублируется в <see cref="Blocks"/>).
        /// Канон: <c>секц:</c>.
        /// </summary>
        public IList<string> Sections { get; } = new List<string>();

        /// <summary>
        /// Абонентский ящик («А/Я»).
        /// Канон: <c>а/я:</c>.
        /// </summary>
        public IList<string> Mailboxes { get; } = new List<string>();

        /// <summary>
        /// Литера здания/корпуса («ЛИТЕРА», «ЛИТ»).
        /// Канон: <c>лит:</c>.
        /// </summary>
        public IList<string> Literas { get; } = new List<string>();

        /// <summary>
        /// Диапазон номеров без явного типа.
        /// Канон: <c>диап:</c>.
        /// </summary>
        public IList<string> Ranges { get; } = new List<string>();

        /// <summary>
        /// Коды без распознанного маркера типа.
        /// Канон: <c>code:</c>.
        /// </summary>
        public IList<string> RawCodes { get; } = new List<string>();

        /// <summary>
        /// Текстовые примечания.
        /// Канон: <c>note:</c>.
        /// </summary>
        public IList<string> Notes { get; } = new List<string>();

        /// <summary>
        /// Неразобранные фрагменты.
        /// Канон: <c>unparsed:</c>.
        /// </summary>
        public IList<string> Unparsed { get; } = new List<string>();
    }
}

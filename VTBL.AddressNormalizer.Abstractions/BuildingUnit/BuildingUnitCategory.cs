namespace VTBL.AddressNormalizer.Abstractions.BuildingUnit
{
    /// <summary>
    /// Категория indoor/unit-строки по доминирующему формату.
    /// </summary>
    /// <remarks>
    /// Определяется классификатором по наличию маркеров в сырой строке,
    /// без разбора значений. Не путать с полями <see cref="BuildingUnitLocation"/> — парсер может
    /// успешно извлечь канон и при <see cref="Unknown"/>.
    /// <para>Порядок приоритета классификатора: Garbage → ServiceMarker → Apartment → Cabinet →
    /// Mixed (3+ типа) → Premise → Office → Room → Floor → Unknown.</para>
    /// </remarks>
    public enum BuildingUnitCategory
    {
        /// <summary>
        /// Помещение — доминирующий маркер «ПОМ», «ПОМЕЩ.», «ПОМЕЩЕНИЕ», «НЕЖ.ПОМ».
        /// </summary>
        /// <remarks>Приоритетная категория при одном типе или двух (напр. «ЭТ/ПОМ 1/40»).</remarks>
        Premise,

        /// <summary>
        /// Офис — доминирующий маркер «ОФ», «ОФИС».
        /// </summary>
        /// <remarks>Напр. «ОФИС 104», «ПОДЪЕЗД 5, ОФИС 1», «ОФИС 301 ЭТАЖ 3».</remarks>
        Office,

        /// <summary>
        /// Комната — доминирующий маркер «КОМ», «КОМН.», «КОМНАТА», «К.».
        /// </summary>
        /// <remarks>Напр. «К. 5-20», «КВ. 10, КОМ. 3,4» (два маркера → Room).</remarks>
        Room,

        /// <summary>
        /// Смешанный формат — три и более разных типа локации в одной строке.
        /// </summary>
        /// <remarks>Напр. «ЭТАЖ/ПОМЕЩ.-КОМ./ОФИС 5/XII-8/34». Два типа — не Mixed.</remarks>
        Mixed,

        /// <summary>
        /// Только этаж — маркеры «ЭТ», «ЭТАЖ», «ПОДВАЛ», «ЦОКОЛЬНЫЙ» без пом/ком/оф.
        /// </summary>
        /// <remarks>Напр. «ЭТАЖ 23», «ЭТ 1-Й».</remarks>
        Floor,

        /// <summary>
        /// Только кабинет — «КАБ», «КАБИНЕТ» без пом/ком/оф/эт.
        /// </summary>
        /// <remarks>Напр. «КАБИНЕТ 69».</remarks>
        Cabinet,

        /// <summary>
        /// Только квартира — «КВ», «КВАРТИРА» без пом/ком/оф/эт/каб.
        /// </summary>
        /// <remarks>Напр. «КВАРТИРА 837».</remarks>
        Apartment,

        /// <summary>
        /// Служебный маркер — «А/Я», «ПОДЪЕЗД» без типизированной локации.
        /// </summary>
        /// <remarks>Напр. «А/Я 165», «ПОДЪЕЗД 5». Блок/секция не считаются локацией.</remarks>
        ServiceMarker,

        /// <summary>
        /// Мусор источника даты, пустые значения.
        /// </summary>
        /// <remarks>«06.10.2007». Хешируется через <c>unparsed:*</c>.</remarks>
        Garbage,

        /// <summary>
        /// Нет распознанных маркеров типа; голые коды и произвольный текст.
        /// </summary>
        /// <remarks>«659318», «74 - 82», «210 (БЦ …)». Канон может быть заполнен через <c>code:</c>.</remarks>
        Unknown,
    }
}

namespace VTBL.AddressNormalizer.Abstractions.Shared
{
    /// <summary>
    /// Категория indoor-маркера для отсечения локации внутри здания.
    /// </summary>
    public enum IndoorMarkerKind
    {
        /// <summary>
        /// Квартира (кв, квартира).
        /// </summary>
        Apartment,

        /// <summary>
        /// Офис (оф, офис).
        /// </summary>
        Office,

        /// <summary>
        /// Помещение (пом, помещ, помещение).
        /// </summary>
        Premise,

        /// <summary>
        /// Комната (ком, комн, комната).
        /// </summary>
        Room,

        /// <summary>
        /// Кабинет (каб, кабинет).
        /// </summary>
        Cabinet,

        /// <summary>
        /// Этаж (эт, этаж).
        /// </summary>
        Floor,

        /// <summary>
        /// Подъезд.
        /// </summary>
        Entrance,

        /// <summary>
        /// Проезд (проезд, пр-д).
        /// </summary>
        Passage,

        /// <summary>
        /// Владение (владение, влад, вл.).
        /// </summary>
        Holding,

        /// <summary>
        /// Склад (склад, скл.).
        /// </summary>
        Storage,

        /// <summary>
        /// Блок.
        /// </summary>
        Block,

        /// <summary>
        /// Секция.
        /// </summary>
        Section,

        /// <summary>
        /// Рабочее место (раб.м).
        /// </summary>
        Workplace,

        /// <summary>
        /// Часть помещения (ч.п).
        /// </summary>
        Part,

        /// <summary>
        /// Абонентский ящик (а/я).
        /// </summary>
        Mailbox,
    }
}

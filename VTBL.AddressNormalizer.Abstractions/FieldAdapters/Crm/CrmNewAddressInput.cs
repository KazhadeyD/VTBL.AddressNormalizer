namespace VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm
{
    /// <summary>
    /// Вход CRM-адаптера для полного адреса (сборка из столбцов <c>new_address</c>).
    /// </summary>
    /// <remarks>Stub для UC-04 (фаза 2). Поля могут расширяться при реализации.</remarks>
    public sealed class CrmNewAddressInput
    {
        /// <summary>Регион.</summary>
        public string Region { get; set; }

        /// <summary>Район / область.</summary>
        public string Area { get; set; }

        /// <summary>Населённый пункт / город.</summary>
        public string City { get; set; }

        /// <summary>Улица.</summary>
        public string Street { get; set; }

        /// <summary>Дом.</summary>
        public string House { get; set; }

        /// <summary>Корпус.</summary>
        public string Corp { get; set; }

        /// <summary>Квартира / помещение (indoor; не входит в building location).</summary>
        public string Flat { get; set; }
    }
}

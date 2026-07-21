using VTBL.AddressNormalizer.Abstractions.Shared;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingAddress
{
    /// <summary>
    /// Реестр синонимов географических типов адреса (§4.3 ТЗ).
    /// </summary>
    internal static class GeographicTypeSynonymRules
    {
        /// <summary>
        /// Все правила замены; длинные формы первыми.
        /// </summary>
        public static SynonymRule[] All { get; } =
        {
            new SynonymRule("город|гор|г", "г"),
            new SynonymRule("улица|ул", "ул"),
            new SynonymRule("дом|д", "д"),
            new SynonymRule("корпус|корп", "корп"),
            new SynonymRule("строение|стр", "стр"),
            new SynonymRule("литера|лит", "лит"),
            new SynonymRule("проспект|просп|пр-кт|пр", "пр-кт"),
            new SynonymRule("переулок|пер", "пер"),
            new SynonymRule("бульвар|бул|б-р", "б-р"),
            new SynonymRule("шоссе|ш", "ш"),
            new SynonymRule("область|обл", "обл"),
            new SynonymRule("район|р-н", "р-н"),
            new SynonymRule("посёлок|поселок|пос|п", "п"),
            new SynonymRule("село|с", "с"),
        };
    }
}

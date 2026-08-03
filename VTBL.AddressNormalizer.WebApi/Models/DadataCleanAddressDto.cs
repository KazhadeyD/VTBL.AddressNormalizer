using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VTBL.AddressNormalizer.WebApi.Models
{
    /// <summary>
    /// Результат очистки и нормализации building-части адреса через DaData clean/address.
    /// Без suggest-only полей (<c>history_values</c>, <c>room_fias_id</c>, <c>room_cadnum</c>).
    /// </summary>
    public class DadataCleanAddressDto
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("country_iso_code")]
        public string CountryIsoCode { get; set; }

        [JsonPropertyName("federal_district")]
        public string FederalDistrict { get; set; }

        [JsonPropertyName("region_fias_id")]
        public string RegionFiasId { get; set; }

        [JsonPropertyName("region_kladr_id")]
        public string RegionKladrId { get; set; }

        [JsonPropertyName("region_iso_code")]
        public string RegionIsoCode { get; set; }

        [JsonPropertyName("region_with_type")]
        public string RegionWithType { get; set; }

        [JsonPropertyName("region_type")]
        public string RegionType { get; set; }

        [JsonPropertyName("region_type_full")]
        public string RegionTypeFull { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("area_fias_id")]
        public string AreaFiasId { get; set; }

        [JsonPropertyName("area_kladr_id")]
        public string AreaKladrId { get; set; }

        [JsonPropertyName("area_with_type")]
        public string AreaWithType { get; set; }

        [JsonPropertyName("area_type")]
        public string AreaType { get; set; }

        [JsonPropertyName("area_type_full")]
        public string AreaTypeFull { get; set; }

        [JsonPropertyName("area")]
        public string Area { get; set; }

        [JsonPropertyName("city_fias_id")]
        public string CityFiasId { get; set; }

        [JsonPropertyName("city_kladr_id")]
        public string CityKladrId { get; set; }

        [JsonPropertyName("city_with_type")]
        public string CityWithType { get; set; }

        [JsonPropertyName("city_type")]
        public string CityType { get; set; }

        [JsonPropertyName("city_type_full")]
        public string CityTypeFull { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("city_area")]
        public string CityArea { get; set; }

        [JsonPropertyName("city_district_fias_id")]
        public string CityDistrictFiasId { get; set; }

        [JsonPropertyName("city_district_kladr_id")]
        public string CityDistrictKladrId { get; set; }

        [JsonPropertyName("city_district_with_type")]
        public string CityDistrictWithType { get; set; }

        [JsonPropertyName("city_district_type")]
        public string CityDistrictType { get; set; }

        [JsonPropertyName("city_district_type_full")]
        public string CityDistrictTypeFull { get; set; }

        [JsonPropertyName("city_district")]
        public string CityDistrict { get; set; }

        [JsonPropertyName("settlement_fias_id")]
        public string SettlementFiasId { get; set; }

        [JsonPropertyName("settlement_kladr_id")]
        public string SettlementKladrId { get; set; }

        [JsonPropertyName("settlement_with_type")]
        public string SettlementWithType { get; set; }

        [JsonPropertyName("settlement_type")]
        public string SettlementType { get; set; }

        [JsonPropertyName("settlement_type_full")]
        public string SettlementTypeFull { get; set; }

        [JsonPropertyName("settlement")]
        public string Settlement { get; set; }

        [JsonPropertyName("street_fias_id")]
        public string StreetFiasId { get; set; }

        [JsonPropertyName("street_kladr_id")]
        public string StreetKladrId { get; set; }

        [JsonPropertyName("street_with_type")]
        public string StreetWithType { get; set; }

        [JsonPropertyName("street_type")]
        public string StreetType { get; set; }

        [JsonPropertyName("street_type_full")]
        public string StreetTypeFull { get; set; }

        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonPropertyName("stead_fias_id")]
        public string SteadFiasId { get; set; }

        [JsonPropertyName("stead_kladr_id")]
        public string SteadKladrId { get; set; }

        [JsonPropertyName("stead_cadnum")]
        public string SteadCadnum { get; set; }

        [JsonPropertyName("stead_type")]
        public string SteadType { get; set; }

        [JsonPropertyName("stead_type_full")]
        public string SteadTypeFull { get; set; }

        [JsonPropertyName("stead")]
        public string Stead { get; set; }

        [JsonPropertyName("house_fias_id")]
        public string HouseFiasId { get; set; }

        [JsonPropertyName("house_kladr_id")]
        public string HouseKladrId { get; set; }

        [JsonPropertyName("house_cadnum")]
        public string HouseCadnum { get; set; }

        [JsonPropertyName("house_flat_count")]
        public string HouseFlatCount { get; set; }

        [JsonPropertyName("house_type")]
        public string HouseType { get; set; }

        [JsonPropertyName("house_type_full")]
        public string HouseTypeFull { get; set; }

        [JsonPropertyName("house")]
        public string House { get; set; }

        [JsonPropertyName("block_type")]
        public string BlockType { get; set; }

        [JsonPropertyName("block_type_full")]
        public string BlockTypeFull { get; set; }

        [JsonPropertyName("block")]
        public string Block { get; set; }

        [JsonPropertyName("entrance")]
        public string Entrance { get; set; }

        [JsonPropertyName("floor")]
        public string Floor { get; set; }

        [JsonPropertyName("flat_fias_id")]
        public string FlatFiasId { get; set; }

        [JsonPropertyName("flat_cadnum")]
        public string FlatCadnum { get; set; }

        [JsonPropertyName("flat_type")]
        public string FlatType { get; set; }

        [JsonPropertyName("flat_type_full")]
        public string FlatTypeFull { get; set; }

        [JsonPropertyName("flat")]
        public string Flat { get; set; }

        [JsonPropertyName("flat_area")]
        public string FlatArea { get; set; }

        [JsonPropertyName("square_meter_price")]
        public string SquareMeterPrice { get; set; }

        [JsonPropertyName("flat_price")]
        public string FlatPrice { get; set; }

        [JsonPropertyName("postal_box")]
        public string PostalBox { get; set; }

        [JsonPropertyName("room_type")]
        public string RoomType { get; set; }

        [JsonPropertyName("room_type_full")]
        public string RoomTypeFull { get; set; }

        [JsonPropertyName("room")]
        public string Room { get; set; }

        [JsonPropertyName("fias_id")]
        public string FiasId { get; set; }

        [JsonPropertyName("fias_code")]
        public string FiasCode { get; set; }

        [JsonPropertyName("fias_level")]
        public string FiasLevel { get; set; }

        [JsonPropertyName("fias_actuality_state")]
        public string FiasActualityState { get; set; }

        [JsonPropertyName("kladr_id")]
        public string KladrId { get; set; }

        [JsonPropertyName("geoname_id")]
        public string GeonameId { get; set; }

        [JsonPropertyName("capital_marker")]
        public string CapitalMarker { get; set; }

        [JsonPropertyName("okato")]
        public string Okato { get; set; }

        [JsonPropertyName("oktmo")]
        public string Oktmo { get; set; }

        [JsonPropertyName("tax_office")]
        public string TaxOffice { get; set; }

        [JsonPropertyName("tax_office_legal")]
        public string TaxOfficeLegal { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("geo_lat")]
        public string GeoLat { get; set; }

        [JsonPropertyName("geo_lon")]
        public string GeoLon { get; set; }

        [JsonPropertyName("beltway_hit")]
        public string BeltwayHit { get; set; }

        [JsonPropertyName("beltway_distance")]
        public string BeltwayDistance { get; set; }

        [JsonPropertyName("metro")]
        public IList<DadataMetroDto> Metro { get; set; }

        [JsonPropertyName("divisions")]
        public DadataAddressDivisionsDto Divisions { get; set; }

        [JsonPropertyName("qc_geo")]
        public int? QcGeo { get; set; }

        [JsonPropertyName("qc_complete")]
        public int? QcComplete { get; set; }

        [JsonPropertyName("qc_house")]
        public int? QcHouse { get; set; }

        [JsonPropertyName("unparsed_parts")]
        public string UnparsedParts { get; set; }

        [JsonPropertyName("qc")]
        public int? Qc { get; set; }
    }
}

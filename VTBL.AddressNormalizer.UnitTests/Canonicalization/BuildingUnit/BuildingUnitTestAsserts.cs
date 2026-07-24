using System;
using System.Collections.Generic;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using Xunit;

namespace VTBL.AddressNormalizer.UnitTests.Canonicalization.BuildingUnit
{
    /// <summary>
    /// Единый assert-path Parse → ToCanonical для BuildingUnit-тестов (слои B/D/E).
    /// </summary>
    internal static class BuildingUnitTestAsserts
    {
        /// <summary>
        /// Разбирает <paramref name="input"/> через Host и сверяет канон с ожидаемым.
        /// </summary>
        public static void AssertCanonical(string input, string expectedCanonical)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);
            Assert.Equal(expectedCanonical, canonical);
        }

        /// <summary>
        /// Сверяет канон и проверяет, что указанные коллекции <see cref="BuildingUnitLocation"/> пусты.
        /// </summary>
        /// <param name="input">Исходная unit-строка.</param>
        /// <param name="expectedCanonical">Ожидаемый результат <c>ToCanonical</c>.</param>
        /// <param name="emptyNeighborCollections">Имена коллекций локации (<c>Rooms</c>, <c>Offices</c>, …).</param>
        public static void AssertCanonicalAndFields(
            string input,
            string expectedCanonical,
            string[] emptyNeighborCollections)
        {
            var location = AddressNormalizerTestHost.Parser.Parse(input);
            var canonical = AddressNormalizerTestHost.Canonicalizer.ToCanonical(location);
            Assert.Equal(expectedCanonical, canonical);

            if (emptyNeighborCollections == null)
            {
                return;
            }

            foreach (var collectionName in emptyNeighborCollections)
            {
                Assert.Empty(GetCollection(location, collectionName));
            }
        }

        private static IList<string> GetCollection(BuildingUnitLocation location, string collectionName)
        {
            return collectionName switch
            {
                nameof(BuildingUnitLocation.Floors) => location.Floors,
                nameof(BuildingUnitLocation.Premises) => location.Premises,
                nameof(BuildingUnitLocation.Rooms) => location.Rooms,
                nameof(BuildingUnitLocation.Offices) => location.Offices,
                nameof(BuildingUnitLocation.Workplaces) => location.Workplaces,
                nameof(BuildingUnitLocation.Parts) => location.Parts,
                nameof(BuildingUnitLocation.Apartments) => location.Apartments,
                nameof(BuildingUnitLocation.Cabinets) => location.Cabinets,
                nameof(BuildingUnitLocation.Entrances) => location.Entrances,
                nameof(BuildingUnitLocation.Passages) => location.Passages,
                nameof(BuildingUnitLocation.Holdings) => location.Holdings,
                nameof(BuildingUnitLocation.Storages) => location.Storages,
                nameof(BuildingUnitLocation.Blocks) => location.Blocks,
                nameof(BuildingUnitLocation.Sections) => location.Sections,
                nameof(BuildingUnitLocation.Mailboxes) => location.Mailboxes,
                nameof(BuildingUnitLocation.Literas) => location.Literas,
                nameof(BuildingUnitLocation.Ranges) => location.Ranges,
                nameof(BuildingUnitLocation.RawCodes) => location.RawCodes,
                nameof(BuildingUnitLocation.Notes) => location.Notes,
                nameof(BuildingUnitLocation.Unparsed) => location.Unparsed,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(collectionName),
                    collectionName,
                    "Неизвестная коллекция BuildingUnitLocation."),
            };
        }
    }
}

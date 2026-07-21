using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.UnitTests;
using VTBL.AddressNormalizer.Abstractions.FieldAdapters.Crm;
using Xunit;
using Xunit.Abstractions;

namespace VTBL.AddressNormalizer.UnitTests.FieldAdapters.Crm
{
    public class CrmNewFlatCorpusTests
    {
        private readonly ITestOutputHelper _output;

        public CrmNewFlatCorpusTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Corpus_AllRows_ProducesNormalizationReport()
        {
            var corpusPath = Path.Combine(AppContext.BaseDirectory, "Corpus", "flats.csv");
            Assert.True(File.Exists(corpusPath), $"Corpus file not found: {corpusPath}");

            var rows = LoadCorpus(corpusPath);
            Assert.True(rows.Count >= 5000, $"Expected at least 5000 corpus rows, got {rows.Count}");

            var kindCounts = new Dictionary<BuildingUnitCategory, int>();
            var unknownSamples = new List<string>();
            var unparsedSamples = new List<string>();
            var emptyCanonicalSamples = new List<string>();
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var unknownCount = 0;
            var unparsedCount = 0;
            var emptyCanonicalCount = 0;
            var garbageCount = 0;
            var whitespaceCount = 0;

            foreach (var raw in rows)
            {
                unique.Add(raw);

                if (string.IsNullOrWhiteSpace(raw))
                {
                    whitespaceCount++;
                    continue;
                }

                var result = AddressNormalizerTestHost.CrmNewFlat.Normalize(raw);

                kindCounts.TryGetValue(result.Category, out var kindCount);
                kindCounts[result.Category] = kindCount + 1;

                if (result.Category == BuildingUnitCategory.Garbage)
                    garbageCount++;

                if (result.Category == BuildingUnitCategory.Unknown)
                {
                    unknownCount++;
                    if (unknownSamples.Count < 25)
                        unknownSamples.Add($"{raw} => {result.Canonical}");
                }

                var hasUnparsed = result.Location.Unparsed.Count > 0 ||
                                  result.Canonical.IndexOf("unparsed:", StringComparison.Ordinal) >= 0;

                if (hasUnparsed)
                {
                    unparsedCount++;
                    if (unparsedSamples.Count < 25)
                        unparsedSamples.Add($"{raw} => {result.Canonical}");
                }

                if (string.IsNullOrEmpty(result.Canonical) &&
                    result.Category != BuildingUnitCategory.Garbage &&
                    !string.IsNullOrWhiteSpace(raw))
                {
                    emptyCanonicalCount++;
                    if (emptyCanonicalSamples.Count < 25)
                        emptyCanonicalSamples.Add(raw);
                }
            }

            var nonWhitespace = rows.Count - whitespaceCount;
            var effective = nonWhitespace - garbageCount;

            _output.WriteLine("=== flats_raw corpus report ===");
            _output.WriteLine($"Total rows:           {rows.Count}");
            _output.WriteLine($"Unique values:        {unique.Count}");
            _output.WriteLine($"Whitespace-only:      {whitespaceCount}");
            _output.WriteLine($"Garbage (classifier): {garbageCount}");
            _output.WriteLine($"Effective rows:       {effective} (non-whitespace, non-garbage)");
            _output.WriteLine(string.Empty);
            _output.WriteLine($"Unknown kind:         {unknownCount} ({Pct(unknownCount, effective)}% of effective)");
            _output.WriteLine($"With unparsed:*:      {unparsedCount} ({Pct(unparsedCount, effective)}% of effective)");
            _output.WriteLine($"Empty canonical:      {emptyCanonicalCount} ({Pct(emptyCanonicalCount, effective)}% of effective)");
            _output.WriteLine(string.Empty);
            _output.WriteLine("Kind distribution (non-whitespace):");
            foreach (var pair in kindCounts.OrderByDescending(p => p.Value))
                _output.WriteLine($"  {pair.Key,-14} {pair.Value,5} ({Pct(pair.Value, nonWhitespace)}%)");

            WriteSamples(_output, "Unknown samples", unknownSamples);
            WriteSamples(_output, "Unparsed samples", unparsedSamples);
            WriteSamples(_output, "Empty canonical samples", emptyCanonicalSamples);

            Assert.Equal(0, emptyCanonicalCount);
        }

        private static void WriteSamples(ITestOutputHelper output, string title, IReadOnlyList<string> samples)
        {
            output.WriteLine(string.Empty);
            output.WriteLine(title + ":");
            if (samples.Count == 0)
            {
                output.WriteLine("  (none)");
                return;
            }

            foreach (var sample in samples)
                output.WriteLine("  " + sample);
        }

        private static string Pct(int part, int total)
        {
            if (total <= 0)
                return "0.0";

            return (100.0 * part / total).ToString("F1");
        }

        private static List<string> LoadCorpus(string path)
        {
            var rows = new List<string>();
            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var value = line.Split(';')[0];
                if (value.Equals("new_flat", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (value.StartsWith("---", StringComparison.Ordinal))
                    continue;

                rows.Add(value);
            }

            return rows;
        }
    }
}

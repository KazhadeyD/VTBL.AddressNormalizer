using System;
using System.Linq;
using VTBL.AddressNormalizer.WebApi.Models;

namespace VTBL.AddressNormalizer.UnitTests.WebApi
{
    /// <summary>
    /// Хелперы для assert по sparse <c>marks</c> в <see cref="IndoorValueDto"/>.
    /// </summary>
    internal static class IndoorValueTestHelper
    {
        public static IndoorMarkDto GetMark(IndoorValueDto indoor, string id)
        {
            if (indoor?.Marks == null)
                return null;

            return indoor.Marks.FirstOrDefault(mark =>
                string.Equals(mark.Id, id, StringComparison.Ordinal));
        }

        public static string[] GetMarkValues(IndoorValueDto indoor, string id) =>
            GetMark(indoor, id)?.Values ?? Array.Empty<string>();
    }
}

using System.Text.RegularExpressions;

namespace Services.Helpers
{
    public static partial class RfcValidator
    {
        private static readonly HashSet<string> GenericRfcs = new(StringComparer.OrdinalIgnoreCase)
        {
            "XAXX010101000",
            "XEXX010101000"
        };

        public static bool IsGeneric(string rfc)
        {
            return GenericRfcs.Contains(Normalize(rfc));
        }

        public static bool IsValid(string rfc)
        {
            if (string.IsNullOrWhiteSpace(rfc))
            {
                return false;
            }

            return RfcRegex().IsMatch(Normalize(rfc));
        }

        public static string Normalize(string rfc)
        {
            return rfc.Trim().ToUpperInvariant();
        }

        [GeneratedRegex(@"^([A-Z&Ñ]{3,4})\d{6}[A-Z0-9]{3}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex RfcRegex();
    }
}

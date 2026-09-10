using System.Globalization;

namespace Services.Helpers
{
    internal static class SpanishDateHelper
    {
        private static readonly CultureInfo Culture = new("es-MX");

        public static string FormatLong(DateTime dt)            => BuildDate(dt);
        public static string FormatLong(DateTimeOffset dt)      => BuildDate(dt.DateTime);

        public static string FormatLongWithTime(DateTime dt)       => $"{BuildDate(dt)} {dt.ToString("hh:mm tt", CultureInfo.InvariantCulture)}";
        public static string FormatLongWithTime(DateTimeOffset dt) => $"{BuildDate(dt.DateTime)} {dt.ToString("hh:mm tt", CultureInfo.InvariantCulture)}";

        public static string FormatShort(DateTime dt)       => BuildDate(dt);
        public static string FormatShort(DateTimeOffset dt) => BuildDate(dt.DateTime);

        private static string BuildDate(DateTime dt)
        {
            var monthName = Culture.DateTimeFormat.GetAbbreviatedMonthName(dt.Month).TrimEnd('.');
            var month     = char.ToUpper(monthName[0]) + monthName[1..];
            return $"{dt:dd}/{month}/{dt:yyyy}";
        }
    }
}

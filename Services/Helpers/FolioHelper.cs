namespace Services.Helpers
{
    public static class FolioHelper
    {
        public static string Build(string docType, string Prefix, int year, int sequence)
            => $"{docType}-{Prefix}{year % 100:D2}-{sequence}";
    }
}

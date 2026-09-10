namespace Services.Constants
{
    public static class ParameterNames
    {
        public const string SmtpHost = "SmtpHost";
        public const string SmtpPort = "SmtpPort";
        public const string SmtpUsername = "SmtpUsername";
        public const string SmtpPassword = "SmtpPassword";
        public const string SmtpFromEmail = "SmtpFromEmail";
        public const string SmtpFromName = "SmtpFromName";
        public const string SmtpEnableSsl = "SmtpEnableSsl";

        public const string JwtExpirationMinutes = "JwtExpirationMinutes";

        public static readonly ISet<string> Sensitive = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            SmtpPassword
        };

        public static readonly IReadOnlyCollection<string> SmtpSettings = new[]
        {
            SmtpHost,
            SmtpPort,
            SmtpUsername,
            SmtpPassword,
            SmtpFromEmail,
            SmtpFromName,
            SmtpEnableSsl
        };
    }
}

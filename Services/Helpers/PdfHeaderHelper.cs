using System.Reflection;

namespace Services.Helpers
{
    /// <summary>
    /// Draws the standard DEMO PDF header: Jael logo (top-left) and
    /// "Generado: dd/MM/yyyy HH:mm — Usuario: <name>" (top-right).
    /// Call once per page (after NewPage) before writing content.
    /// </summary>
    internal static class PdfHeaderHelper
    {
        public const float HeaderBottomY = 730f;

        private static readonly Lazy<byte[]> LogoBytes = new(LoadLogo);

        /// <summary>Raw image bytes (JPEG or PNG) of the Jael logo. Use with PdfBuilder.RegisterImage for non-standard layouts (ticket-size PDFs).</summary>
        public static byte[] LogoBytes_Raw => LogoBytes.Value;

        private static byte[] LoadLogo()
        {
            const string resourceName = "Services.Assets.logo.png";
            var asm = typeof(PdfHeaderHelper).Assembly;
            using var stream = asm.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Embedded resource '{resourceName}' not found. Confirm the EmbeddedResource entry in Services.csproj.");
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Draws logo + metadata header. Returns the Y coordinate where content can start.
        /// </summary>
        public static float DrawHeader(PdfBuilder pdf, string userName, DateTimeOffset generatedAt)
        {
            var imgIdx = pdf.RegisterImage(LogoBytes.Value);

            // Logo: 50w × 40h, top-left
            pdf.DrawImage(imgIdx, PdfBuilder.MarginL, 740f, 50f, 40f);

            var local = generatedAt.ToLocalTime();
            var date  = local.ToString("dd/MM/yyyy HH:mm");
            var user  = string.IsNullOrWhiteSpace(userName) ? "—" : userName;

            pdf.TextRight(PdfBuilder.MarginR, 770f, $"Generado: {date}", PdfBuilder.Regular, 8.5f);
            pdf.TextRight(PdfBuilder.MarginR, 757f, $"Usuario: {user}", PdfBuilder.Regular, 8.5f);

            pdf.HLine(PdfBuilder.MarginL, PdfBuilder.MarginR, 735f, 0.4f, 0.5f);

            return HeaderBottomY;
        }
    }
}

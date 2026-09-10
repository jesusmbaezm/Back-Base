using System.Text;

namespace Services.Helpers
{
    /// <summary>
    /// Converts a decimal currency amount to Spanish words for use in legal documents (pagarés).
    /// Example: 8632.00 → "Ocho Mil Seiscientos Treinta y Dos pesos M.N."
    /// </summary>
    internal static class AmountToWordsHelper
    {
        private static readonly string[] Ones =
        [
            "", "Un", "Dos", "Tres", "Cuatro", "Cinco", "Seis", "Siete",
            "Ocho", "Nueve", "Diez", "Once", "Doce", "Trece", "Catorce",
            "Quince", "Diecis\u00e9is", "Diecisiete", "Dieciocho", "Diecinueve"
        ];

        private static readonly string[] Tens =
        [
            "", "Diez", "Veinte", "Treinta", "Cuarenta", "Cincuenta",
            "Sesenta", "Setenta", "Ochenta", "Noventa"
        ];

        private static readonly string[] Hundreds =
        [
            "", "Ciento", "Doscientos", "Trescientos", "Cuatrocientos",
            "Quinientos", "Seiscientos", "Setecientos", "Ochocientos", "Novecientos"
        ];

        public static string ToSpanishWords(decimal amount)
        {
            var intPart = (long)Math.Truncate(amount);
            var cents   = (int)Math.Round((amount - intPart) * 100);

            var words = Convert(intPart);
            return cents > 0
                ? $"{words} pesos con {cents:D2}/100 M.N."
                : $"{words} pesos M.N.";
        }

        private static string Convert(long n)
        {
            if (n == 0) return "Cero";

            var parts = new List<string>();

            if (n >= 1_000_000)
            {
                var m = n / 1_000_000;
                parts.Add(m == 1 ? "Un Mill\u00f3n" : $"{ConvertHundreds((int)m)} Millones");
                n %= 1_000_000;
            }

            if (n >= 1_000)
            {
                var t = n / 1_000;
                parts.Add(t == 1 ? "Mil" : $"{ConvertHundreds((int)t)} Mil");
                n %= 1_000;
            }

            if (n > 0) parts.Add(ConvertHundreds((int)n));

            return string.Join(" ", parts);
        }

        private static string ConvertHundreds(int n)
        {
            if (n == 0)   return string.Empty;
            if (n == 100) return "Cien";

            var sb = new StringBuilder();
            if (n >= 100)
            {
                sb.Append(Hundreds[n / 100]);
                n %= 100;
                if (n > 0) sb.Append(' ');
            }
            if (n > 0) sb.Append(ConvertTens(n));
            return sb.ToString();
        }

        private static string ConvertTens(int n)
        {
            if (n < 20) return Ones[n];

            if (n < 30)
            {
                string[] v =
                [
                    "Veinte", "Veintiun", "Veintid\u00f3s", "Veintitr\u00e9s", "Veinticuatro",
                    "Veinticinco", "Veintis\u00e9is", "Veintisiete", "Veintiocho", "Veintinueve"
                ];
                return v[n - 20];
            }

            return n % 10 == 0 ? Tens[n / 10] : $"{Tens[n / 10]} y {Ones[n % 10]}";
        }
    }
}

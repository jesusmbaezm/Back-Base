using Data.Enums;

namespace Services.Helpers
{
    public static class DiscountLabelHelper
    {
        public static string ToSpanishLabel(DiscountType type)
        {
            return type switch
            {
                DiscountType.Percentage => "Porcentaje",
                DiscountType.FixedAmount => "Monto fijo",
                _ => type.ToString()
            };
        }

        public static string ToSpanishLabel(DiscountScope scope)
        {
            return scope switch
            {
                DiscountScope.Global => "Global",
                DiscountScope.Category => "Categoria",
                DiscountScope.Product => "Producto",
                _ => scope.ToString()
            };
        }

        public static string ToSpanishLabel(DiscountPaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                DiscountPaymentMethod.Cash => "Contado",
                DiscountPaymentMethod.Credit => "Credito",
                DiscountPaymentMethod.Both => "Ambos",
                _ => paymentMethod.ToString()
            };
        }
    }
}

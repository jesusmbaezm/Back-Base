using Data.Enums;

namespace Services.Helpers
{
    public static class DocumentLabelHelper
    {
        public static string ToSpanishLabel(DocumentEntityType entityType)
        {
            return entityType switch
            {
                DocumentEntityType.Customer => "Cliente",
                DocumentEntityType.Supplier => "Proveedor",
                _ => entityType.ToString()
            };
        }

        public static string ToSpanishLabel(DocumentType documentType)
        {
            return documentType switch
            {
                DocumentType.INE => "INE",
                DocumentType.ProofOfAddress => "Comprobante de Domicilio",
                DocumentType.TaxCertificate => "Constancia Fiscal",
                DocumentType.Other => "Otro",
                _ => documentType.ToString()
            };
        }
    }
}

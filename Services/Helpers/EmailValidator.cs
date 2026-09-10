using System.ComponentModel.DataAnnotations;

namespace Services.Helpers
{
    public static class EmailValidator
    {
        private static readonly EmailAddressAttribute EmailAddressAttribute = new();

        public static bool IsValidOptional(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return true;
            }

            return EmailAddressAttribute.IsValid(email.Trim());
        }
    }
}

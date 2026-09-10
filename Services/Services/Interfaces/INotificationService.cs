namespace Services.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendAuthorizationRequestAsync(string email, string quotationFolio, string customerName, decimal amount, decimal availableCredit);
        Task SendAuthorizationApprovedAsync(string email, string quotationFolio, string customerName, string approvedByName);
        Task SendAuthorizationRejectedAsync(string email, string quotationFolio, string customerName, string rejectedByName, string comments);
        Task SendEarlyPaymentAuthorizationRequestAsync(string email, string customerName, string remissionFolio, int installmentNumber, decimal requestedAmount, string reason);
        Task SendEarlyPaymentAuthorizationApprovedAsync(string email, string customerName, string remissionFolio, int installmentNumber, decimal requestedAmount, string approvedByName);
        Task SendReconciliationAuthorizationRequestAsync(string email, string branchName, DateTimeOffset openedAt, DateTimeOffset? closedAt, string? comment);
        Task SendCustomerReturnAuthorizationRequestAsync(string email, string folio, string customerName, string branchName, string comments);
        Task<bool> SendLowStockAlertAsync(string email, string branchName, string variantSku, string productName, int currentQuantity, int minStock, CancellationToken cancellationToken = default);
        Task SendPasswordResetCodeAsync(string email, string userName, string token, int expirationMinutes);
    }
}

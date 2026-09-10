using Data.Enums;

namespace Services.Helpers
{
    public static class FinanceLabelHelper
    {
        public static string ToSpanishLabel(PeriodStatus status) => status switch
        {
            PeriodStatus.Open => "Abierto",
            PeriodStatus.Closed => "Cerrado",
            PeriodStatus.PendingReconciliation => "Pendiente de conciliacion",
            PeriodStatus.Reconciled => "Conciliado",
            PeriodStatus.NotReconciled => "No conciliado",
            _ => status.ToString()
        };

        public static string ToSpanishLabel(PeriodMovementType type) => type switch
        {
            PeriodMovementType.Income => "Ingreso",
            PeriodMovementType.Expense => "Gasto",
            PeriodMovementType.BankDeposit => "Deposito bancario",
            _ => type.ToString()
        };

        public static string ToSpanishLabel(ExpenseStatus status) => status switch
        {
            ExpenseStatus.PendingApproval => "Pendiente de autorizacion",
            ExpenseStatus.Authorized => "Autorizado",
            ExpenseStatus.Rejected => "Rechazado",
            _ => status.ToString()
        };

        public static string ToSpanishLabel(AccountsReceivablePaymentType type) => type switch
        {
            AccountsReceivablePaymentType.General => "Abono general",
            AccountsReceivablePaymentType.Specific => "Abono especifico",
            AccountsReceivablePaymentType.Bonification => "Bonificacion",
            _ => type.ToString()
        };

        public static string ToSpanishLabel(EarlyPaymentAuthorizationStatus status) => status switch
        {
            EarlyPaymentAuthorizationStatus.Pending => "Pendiente",
            EarlyPaymentAuthorizationStatus.Approved => "Aprobado",
            EarlyPaymentAuthorizationStatus.Rejected => "Rechazado",
            _ => status.ToString()
        };

        public static string ToSpanishLabel(CommissionStatus status) => status switch
        {
            CommissionStatus.Pending => "Pendiente",
            CommissionStatus.Paid => "Pagada",
            CommissionStatus.Cancelled => "Cancelada",
            _ => status.ToString()
        };

        public static string ToSpanishLabel(CommissionType type) => type switch
        {
            CommissionType.General => "General",
            CommissionType.Product => "Por producto",
            _ => type.ToString()
        };

        public static string ToSpanishLabel(CommissionPayoutStatus status) => status switch
        {
            CommissionPayoutStatus.PendingPayment => "Pendiente de pago",
            CommissionPayoutStatus.Paid => "Pagada",
            CommissionPayoutStatus.Voided => "Anulada",
            _ => status.ToString()
        };

        public static string ToSpanishLabel(CashPaymentMethod method) => method switch
        {
            CashPaymentMethod.Cash => "Efectivo",
            CashPaymentMethod.Card => "Tarjeta",
            CashPaymentMethod.Transfer => "Transferencia",
            _ => method.ToString()
        };
    }
}

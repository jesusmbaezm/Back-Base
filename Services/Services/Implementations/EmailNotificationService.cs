using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using Services.Services.Interfaces;

namespace Services.Services.Implementations
{
    public class EmailNotificationService : INotificationService
    {
        private readonly ISmtpSettingsProvider _smtpSettingsProvider;
        private readonly IEmailSmtpClientFactory _smtpClientFactory;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(
            ISmtpSettingsProvider smtpSettingsProvider,
            IEmailSmtpClientFactory smtpClientFactory,
            ILogger<EmailNotificationService> logger)
        {
            _smtpSettingsProvider = smtpSettingsProvider;
            _smtpClientFactory = smtpClientFactory;
            _logger = logger;
        }

        public async Task SendAuthorizationRequestAsync(string email, string quotationFolio, string customerName, decimal amount, decimal availableCredit)
        {
            var excess = amount - availableCredit;
            var subject = $"Autorización requerida - Cotización {quotationFolio}";
            var body = BuildEmailTemplate(
                title: "Solicitud de Autorización",
                iconColor: "#F59E0B",
                iconSvg: WarningIcon,
                content: $@"
                    <p style=""margin:0 0 16px;color:#374151;font-size:15px;line-height:1.6;"">
                        Se ha generado una solicitud de autorización por exceso de crédito que requiere su aprobación.
                    </p>
                    <table style=""width:100%;border-collapse:collapse;margin:0 0 24px;"">
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;width:45%;"">Cotización</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{quotationFolio}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Cliente</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{customerName}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Monto solicitado</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{amount:C2}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Crédito disponible</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{availableCredit:C2}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;background:#FEF2F2;border:1px solid #FECACA;font-size:14px;color:#991B1B;"">Excedente</td>
                            <td style=""padding:12px 16px;background:#FEF2F2;border:1px solid #FECACA;font-size:14px;color:#991B1B;font-weight:700;"">{excess:C2}</td>
                        </tr>
                    </table>
                    <p style=""margin:0;color:#6B7280;font-size:13px;line-height:1.5;"">
                        Ingrese al sistema para aprobar o rechazar esta solicitud.
                    </p>");

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendAuthorizationApprovedAsync(string email, string quotationFolio, string customerName, string approvedByName)
        {
            var subject = $"Cotización aprobada - {quotationFolio}";
            var body = BuildEmailTemplate(
                title: "Cotización Aprobada",
                iconColor: "#10B981",
                iconSvg: CheckIcon,
                content: $@"
                    <p style=""margin:0 0 16px;color:#374151;font-size:15px;line-height:1.6;"">
                        La solicitud de autorización ha sido <strong style=""color:#059669;"">aprobada</strong>. La cotización ya puede continuar su proceso.
                    </p>
                    <table style=""width:100%;border-collapse:collapse;margin:0 0 24px;"">
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;width:45%;"">Cotización</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{quotationFolio}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Cliente</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{customerName}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Aprobado por</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{approvedByName}</td>
                        </tr>
                    </table>");

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendAuthorizationRejectedAsync(string email, string quotationFolio, string customerName, string rejectedByName, string comments)
        {
            var subject = $"Cotización rechazada - {quotationFolio}";
            var body = BuildEmailTemplate(
                title: "Cotización Rechazada",
                iconColor: "#EF4444",
                iconSvg: RejectIcon,
                content: $@"
                    <p style=""margin:0 0 16px;color:#374151;font-size:15px;line-height:1.6;"">
                        La solicitud de autorización ha sido <strong style=""color:#DC2626;"">rechazada</strong>.
                    </p>
                    <table style=""width:100%;border-collapse:collapse;margin:0 0 24px;"">
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;width:45%;"">Cotización</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{quotationFolio}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Cliente</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{customerName}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Rechazado por</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{rejectedByName}</td>
                        </tr>
                    </table>
                    <div style=""background:#FEF2F2;border:1px solid #FECACA;border-radius:8px;padding:16px;margin:0 0 16px;"">
                        <p style=""margin:0 0 4px;color:#991B1B;font-size:13px;font-weight:600;text-transform:uppercase;letter-spacing:0.05em;"">Motivo del rechazo</p>
                        <p style=""margin:0;color:#7F1D1D;font-size:14px;line-height:1.5;"">{comments}</p>
                    </div>");

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEarlyPaymentAuthorizationRequestAsync(string email, string customerName, string remissionFolio, int installmentNumber, decimal requestedAmount, string reason)
        {
            var subject = $"Autorizacion requerida - Pronto pago {remissionFolio}";
            var body = BuildEmailTemplate(
                title: "Solicitud de Autorizacion de Pronto Pago",
                iconColor: "#F59E0B",
                iconSvg: WarningIcon,
                content: $@"
                    <p style=""margin:0 0 16px;color:#374151;font-size:15px;line-height:1.6;"">
                        Se ha generado una solicitud de autorizacion excepcional de pronto pago que requiere su revision.
                    </p>
                    <table style=""width:100%;border-collapse:collapse;margin:0 0 24px;"">
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;width:45%;"">Cliente</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{customerName}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Remision</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{remissionFolio}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Parcialidad</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{installmentNumber}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Monto solicitado</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{requestedAmount:C2}</td>
                        </tr>
                    </table>
                    <div style=""background:#FFFBEB;border:1px solid #FDE68A;border-radius:8px;padding:16px;margin:0 0 16px;"">
                        <p style=""margin:0 0 4px;color:#92400E;font-size:13px;font-weight:600;text-transform:uppercase;letter-spacing:0.05em;"">Motivo</p>
                        <p style=""margin:0;color:#78350F;font-size:14px;line-height:1.5;"">{reason}</p>
                    </div>
                    <p style=""margin:0;color:#6B7280;font-size:13px;line-height:1.5;"">
                        Ingrese al sistema para aprobar o rechazar esta solicitud.
                    </p>");

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEarlyPaymentAuthorizationApprovedAsync(string email, string customerName, string remissionFolio, int installmentNumber, decimal requestedAmount, string approvedByName)
        {
            var subject = $"Pronto pago aprobado - {remissionFolio}";
            var body = BuildEmailTemplate(
                title: "Autorizacion de Pronto Pago Aprobada",
                iconColor: "#10B981",
                iconSvg: CheckIcon,
                content: $@"
                    <p style=""margin:0 0 16px;color:#374151;font-size:15px;line-height:1.6;"">
                        Tu solicitud de autorizacion de pronto pago ha sido <strong style=""color:#059669;"">aprobada</strong> y el abono ya fue generado automaticamente.
                    </p>
                    <table style=""width:100%;border-collapse:collapse;margin:0 0 24px;"">
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;width:45%;"">Cliente</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{customerName}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Remision</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{remissionFolio}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Parcialidad</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{installmentNumber}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Monto aplicado</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{requestedAmount:C2}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Aprobado por</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{approvedByName}</td>
                        </tr>
                    </table>");

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendReconciliationAuthorizationRequestAsync(string email, string branchName, DateTimeOffset openedAt, DateTimeOffset? closedAt, string? comment)
        {
            var subject = $"Autorización requerida - Conciliación {branchName}";
            var closedRow = closedAt.HasValue
                ? $@"<tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Fecha de cierre</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{closedAt.Value.ToLocalTime():dd/MM/yyyy HH:mm}</td>
                        </tr>"
                : string.Empty;
            var commentBlock = !string.IsNullOrWhiteSpace(comment)
                ? $@"<div style=""background:#FFFBEB;border:1px solid #FDE68A;border-radius:8px;padding:16px;margin:0 0 16px;"">
                        <p style=""margin:0 0 4px;color:#92400E;font-size:13px;font-weight:600;text-transform:uppercase;letter-spacing:0.05em;"">Comentario</p>
                        <p style=""margin:0;color:#78350F;font-size:14px;line-height:1.5;"">{comment}</p>
                    </div>"
                : string.Empty;

            var body = BuildEmailTemplate(
                title: "Solicitud de Autorización de Conciliación",
                iconColor: "#F59E0B",
                iconSvg: WarningIcon,
                content: $@"
                    <p style=""margin:0 0 16px;color:#374151;font-size:15px;line-height:1.6;"">
                        Un periodo de caja ha sido enviado a conciliación y requiere su autorización.
                    </p>
                    <table style=""width:100%;border-collapse:collapse;margin:0 0 24px;"">
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;width:45%;"">Sucursal</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{branchName}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Fecha de apertura</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{openedAt.ToLocalTime():dd/MM/yyyy HH:mm}</td>
                        </tr>
                        {closedRow}
                    </table>
                    {commentBlock}
                    <p style=""margin:0;color:#6B7280;font-size:13px;line-height:1.5;"">
                        Ingrese al sistema para revisar y autorizar o rechazar esta conciliación.
                    </p>");

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendCustomerReturnAuthorizationRequestAsync(string email, string folio, string customerName, string branchName, string comments)
        {
            var subject = $"Autorización requerida - Devolución {folio}";
            var body = BuildEmailTemplate(
                title: "Solicitud de Autorización de Devolución",
                iconColor: "#F59E0B",
                iconSvg: WarningIcon,
                content: $@"
                    <p style=""margin:0 0 16px;color:#374151;font-size:15px;line-height:1.6;"">
                        Se ha registrado una devolución de cliente que requiere su aprobación.
                    </p>
                    <table style=""width:100%;border-collapse:collapse;margin:0 0 24px;"">
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;width:45%;"">Folio</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{folio}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Cliente</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{customerName}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Sucursal</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{branchName}</td>
                        </tr>
                    </table>
                    <div style=""background:#FFFBEB;border:1px solid #FDE68A;border-radius:8px;padding:16px;margin:0 0 16px;"">
                        <p style=""margin:0 0 4px;color:#92400E;font-size:13px;font-weight:600;text-transform:uppercase;letter-spacing:0.05em;"">Comentarios</p>
                        <p style=""margin:0;color:#78350F;font-size:14px;line-height:1.5;"">{comments}</p>
                    </div>
                    <p style=""margin:0;color:#6B7280;font-size:13px;line-height:1.5;"">
                        Ingrese al sistema para aprobar o rechazar esta devolución.
                    </p>");

            await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendLowStockAlertAsync(string email, string branchName, string variantSku, string productName, int currentQuantity, int minStock, CancellationToken cancellationToken = default)
        {
            var subject = $"Alerta de stock bajo - {variantSku}";
            var body = BuildEmailTemplate(
                title: "Alerta de Stock Mínimo",
                iconColor: "#F59E0B",
                iconSvg: WarningIcon,
                content: $@"
                    <p style=""margin:0 0 16px;color:#374151;font-size:15px;line-height:1.6;"">
                        El stock disponible de un producto ha caído por debajo del mínimo configurado.
                    </p>
                    <table style=""width:100%;border-collapse:collapse;margin:0 0 24px;"">
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;width:45%;"">Sucursal</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{branchName}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Producto</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{productName}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">SKU</td>
                            <td style=""padding:12px 16px;background:#F9FAFB;border:1px solid #E5E7EB;font-size:14px;color:#111827;font-weight:600;"">{variantSku}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#6B7280;"">Stock actual</td>
                            <td style=""padding:12px 16px;border:1px solid #E5E7EB;font-size:14px;color:#DC2626;font-weight:700;"">{currentQuantity}</td>
                        </tr>
                        <tr>
                            <td style=""padding:12px 16px;background:#FEF2F2;border:1px solid #FECACA;font-size:14px;color:#991B1B;"">Stock mínimo</td>
                            <td style=""padding:12px 16px;background:#FEF2F2;border:1px solid #FECACA;font-size:14px;color:#991B1B;font-weight:700;"">{minStock}</td>
                        </tr>
                    </table>
                    <p style=""margin:0;color:#6B7280;font-size:13px;line-height:1.5;"">
                        Se recomienda realizar un pedido de reposición a la brevedad.
                    </p>");

            return await TrySendEmailAsync(email, subject, body, cancellationToken);
        }

        public async Task SendPasswordResetCodeAsync(string email, string userName, string token, int expirationMinutes)
        {
            var subject = "Recuperacion de contrasena - DEMO";
            var body = BuildEmailTemplate(
                title: "Recuperacion de Contrasena",
                iconColor: "#2563EB",
                iconSvg: KeyIcon,
                content: $@"
                    <p style=""margin:0 0 16px;color:#374151;font-size:15px;line-height:1.6;"">
                        Hola <strong>{userName}</strong>, recibimos una solicitud para restablecer tu contrasena.
                    </p>
                    <p style=""margin:0 0 16px;color:#374151;font-size:15px;line-height:1.6;"">
                        Usa el siguiente codigo de un solo uso en el panel para completar el proceso:
                    </p>
                    <div style=""background:#EFF6FF;border:1px solid #BFDBFE;border-radius:12px;padding:20px;margin:0 0 20px;text-align:center;"">
                        <p style=""margin:0 0 8px;color:#1D4ED8;font-size:12px;letter-spacing:0.12em;text-transform:uppercase;font-weight:700;"">Codigo de recuperacion</p>
                        <p style=""margin:0;color:#1E3A8A;font-size:28px;font-weight:800;letter-spacing:0.2em;"">{token}</p>
                    </div>
                    <p style=""margin:0 0 12px;color:#6B7280;font-size:13px;line-height:1.6;"">
                        El codigo expira en {expirationMinutes} minutos y solo puede usarse una vez.
                    </p>
                    <p style=""margin:0;color:#6B7280;font-size:13px;line-height:1.6;"">
                        Si tu no solicitaste este cambio, puedes ignorar este mensaje.
                    </p>");

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var settings = await _smtpSettingsProvider.GetRequiredSettingsAsync();
                var message = BuildMimeMessage(settings.FromEmail, settings.FromName, to, subject, htmlBody);

                using var client = _smtpClientFactory.CreateClient();
                await client.ConnectAsync(
                    settings.Host,
                    settings.Port,
                    ResolveSecureSocketOptions(settings.EnableSsl, settings.Port));
                await client.AuthenticateAsync(settings.Username, settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Email enviado a {To} - {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email a {To} - {Subject}", to, subject);
            }
        }

        private async Task<bool> TrySendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken)
        {
            try
            {
                var settings = await _smtpSettingsProvider.GetRequiredSettingsAsync();
                var message = BuildMimeMessage(settings.FromEmail, settings.FromName, to, subject, htmlBody);

                using var client = _smtpClientFactory.CreateClient();
                await client.ConnectAsync(
                    settings.Host,
                    settings.Port,
                    ResolveSecureSocketOptions(settings.EnableSsl, settings.Port),
                    cancellationToken);
                await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);
                _logger.LogInformation("Email enviado a {To} - {Subject}", to, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email a {To} - {Subject}", to, subject);
                return false;
            }
        }

        private static MimeMessage BuildMimeMessage(string fromEmail, string fromName, string to, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            message.Body = builder.ToMessageBody();
            return message;
        }

        private static SecureSocketOptions ResolveSecureSocketOptions(bool enableSsl, int port)
        {
            if (!enableSsl)
            {
                return SecureSocketOptions.None;
            }

            return port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;
        }

        private static string BuildEmailTemplate(string title, string iconColor, string iconSvg, string content)
        {
            return $@"<!DOCTYPE html>
                <html lang=""es"">
                <head><meta charset=""UTF-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1.0""></head>
                <body style=""margin:0;padding:0;background:#F3F4F6;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;"">
                  <table role=""presentation"" style=""width:100%;border-collapse:collapse;"">
                    <tr>
                      <td style=""padding:40px 20px;"">
                        <table role=""presentation"" style=""max-width:560px;margin:0 auto;border-collapse:collapse;"">
                          <!-- Header -->
                          <tr>
                            <td style=""background:#1F2937;padding:24px 32px;border-radius:12px 12px 0 0;"">
                              <table style=""width:100%;border-collapse:collapse;"">
                                <tr>
                                  <td>
                                    <h1 style=""margin:0;color:#FFFFFF;font-size:20px;font-weight:700;letter-spacing:-0.025em;"">DEMO</h1>
                                    <p style=""margin:4px 0 0;color:#9CA3AF;font-size:12px;text-transform:uppercase;letter-spacing:0.1em;"">Sistema Integral de Gestión Jael</p>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                          <!-- Body -->
                          <tr>
                            <td style=""background:#FFFFFF;padding:32px;border-left:1px solid #E5E7EB;border-right:1px solid #E5E7EB;"">
                              <!-- Icon + Title -->
                              <table style=""width:100%;border-collapse:collapse;margin:0 0 24px;"">
                                <tr>
                                  <td style=""width:40px;vertical-align:top;padding-right:12px;"">
                                    <div style=""width:40px;height:40px;background:{iconColor}15;border-radius:10px;text-align:center;line-height:40px;"">
                                      {iconSvg}
                                    </div>
                                  </td>
                                  <td style=""vertical-align:middle;"">
                                    <h2 style=""margin:0;color:#111827;font-size:18px;font-weight:700;"">{title}</h2>
                                  </td>
                                </tr>
                              </table>
                              <hr style=""border:none;border-top:1px solid #E5E7EB;margin:0 0 24px;"">
                              {content}
                            </td>
                          </tr>
                          <!-- Footer -->
                          <tr>
                            <td style=""background:#F9FAFB;padding:20px 32px;border-radius:0 0 12px 12px;border:1px solid #E5E7EB;border-top:none;"">
                              <p style=""margin:0;color:#9CA3AF;font-size:12px;text-align:center;line-height:1.5;"">
                                Este es un mensaje automático del sistema DEMO. Por favor no responda a este correo.
                              </p>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>";
        }


        private const string WarningIcon = @"<span style=""color:#F59E0B;font-size:20px;"">&#9888;</span>";
        private const string CheckIcon = @"<span style=""color:#10B981;font-size:20px;"">&#10003;</span>";
        private const string RejectIcon = @"<span style=""color:#EF4444;font-size:20px;"">&#10007;</span>";
        private const string KeyIcon = @"<span style=""color:#2563EB;font-size:20px;"">&#128273;</span>";
    }
}

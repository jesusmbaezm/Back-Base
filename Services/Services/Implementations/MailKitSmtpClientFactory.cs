using Services.Services.Interfaces;
using Services.Services.Implementations;

namespace Services.Services.Implementations;

public class MailKitSmtpClientFactory : IEmailSmtpClientFactory
{
    public IEmailSmtpClient CreateClient()
    {
        return new MailKitSmtpClientAdapter();
    }
}

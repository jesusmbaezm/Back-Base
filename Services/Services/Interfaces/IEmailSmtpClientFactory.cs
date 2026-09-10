namespace Services.Services.Interfaces;

public interface IEmailSmtpClientFactory
{
    IEmailSmtpClient CreateClient();
}

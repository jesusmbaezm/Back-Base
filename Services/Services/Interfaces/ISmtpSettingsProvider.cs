using Services.Settings;

namespace Services.Services.Interfaces
{
    public interface ISmtpSettingsProvider
    {
        Task<SmtpSettings> GetRequiredSettingsAsync();
    }
}

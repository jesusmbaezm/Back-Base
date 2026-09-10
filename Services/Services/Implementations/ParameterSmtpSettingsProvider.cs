using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Services.Constants;
using Services.Services.Interfaces;
using Services.Settings;

namespace Services.Services.Implementations
{
    public class ParameterSmtpSettingsProvider : ISmtpSettingsProvider
    {
        private readonly IParameterRepository _parameterRepository;

        public ParameterSmtpSettingsProvider(IParameterRepository parameterRepository)
        {
            _parameterRepository = parameterRepository;
        }

        public async Task<SmtpSettings> GetRequiredSettingsAsync()
        {
            var parameters = await _parameterRepository.Query()
                .Where(x => ParameterNames.SmtpSettings.Contains(x.Name))
                .ToDictionaryAsync(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

            var host = GetRequiredValue(parameters, ParameterNames.SmtpHost);
            var portValue = GetRequiredValue(parameters, ParameterNames.SmtpPort);
            var username = GetRequiredValue(parameters, ParameterNames.SmtpUsername);
            var password = GetRequiredValue(parameters, ParameterNames.SmtpPassword);
            var fromEmail = GetRequiredValue(parameters, ParameterNames.SmtpFromEmail);
            var fromName = parameters.TryGetValue(ParameterNames.SmtpFromName, out var configuredFromName) &&
                           !string.IsNullOrWhiteSpace(configuredFromName)
                ? configuredFromName
                : "DEMO Sistema";
            var enableSsl = ParseBoolean(
                parameters.TryGetValue(ParameterNames.SmtpEnableSsl, out var configuredEnableSsl)
                    ? configuredEnableSsl
                    : "true",
                ParameterNames.SmtpEnableSsl);

            if (!int.TryParse(portValue, out var port) || port <= 0)
            {
                throw new InvalidOperationException($"El parametro {ParameterNames.SmtpPort} no contiene un entero valido.");
            }

            return new SmtpSettings
            {
                Host = host,
                Port = port,
                Username = username,
                Password = password,
                FromEmail = fromEmail,
                FromName = fromName,
                EnableSsl = enableSsl
            };
        }

        private static string GetRequiredValue(IReadOnlyDictionary<string, string> parameters, string parameterName)
        {
            if (!parameters.TryGetValue(parameterName, out var value) || string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"El parametro {parameterName} no esta configurado.");
            }

            return value;
        }

        private static bool ParseBoolean(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"El parametro {parameterName} no esta configurado.");
            }

            if (bool.TryParse(value, out var parsed))
            {
                return parsed;
            }

            return value.Trim() switch
            {
                "1" => true,
                "0" => false,
                _ => throw new InvalidOperationException($"El parametro {parameterName} no contiene un valor booleano valido.")
            };
        }
    }
}

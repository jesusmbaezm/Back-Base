using Data.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services.Constants;
using Services.DTOs.Auth;
using Services.Services.Interfaces;
using Services.Settings;
using Data.Entities;
using Services.DTOs.Users;
using Services.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Services.Implementations;

public class AuthService : IAuthService
{
    private const int PasswordResetExpirationMinutes = 30;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly JwtSettings _jwtSettings;
    private readonly IParameterRepository _parameterRepository;

    public AuthService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        INotificationService notificationService,
        IAuditLogService auditLogService,
        IOptions<JwtSettings> jwtSettings,
        IParameterRepository parameterRepository)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
        _jwtSettings = jwtSettings.Value;
        _parameterRepository = parameterRepository;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim();

        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var userWithRoles = await _userRepository.GetByIdWithRolesAndPermissionsAsync(user.Id);
        if (userWithRoles is null)
        {
            return null;
        }

        var roles = userWithRoles.UserRoles
            .Select(ur => ur.Role.Name)
            .ToList();

        var permissions = userWithRoles.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        var expirationMinutes = await GetExpirationMinutesAsync();
        var token = GenerateToken(user.Id, user.Email, user.Name, roles, permissions, expirationMinutes);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Name = user.Name,
            Email = user.Email,
            Roles = roles,
            Permissions = permissions,
        };
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string currentToken)
    {
        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear(); // preserve raw JWT claim names (sub, etc.)
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = key
        };

        ClaimsPrincipal principal;
        SecurityToken validatedToken;
        try
        {
            principal = handler.ValidateToken(currentToken, validationParameters, out validatedToken);
        }
        catch (SecurityTokenException)
        {
            throw new ValidationException("Token inválido.");
        }

        var jwtToken = (JwtSecurityToken)validatedToken;
        var expiration = jwtToken.ValidTo;
        var now = DateTime.UtcNow;

        if (expiration < now)
            throw new ValidationException("El token ya expiró. Inicie sesión nuevamente.");

        var remainingMinutes = (expiration - now).TotalMinutes;
        if (remainingMinutes > _jwtSettings.RefreshWindowMinutes)
            throw new ValidationException("El token aún no es elegible para renovación.");

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new ValidationException("Token inválido.");

        var user = await _userRepository.GetByIdWithRolesAndPermissionsAsync(userId)
            ?? throw new ValidationException("Usuario no encontrado o inactivo.");

        var roles = user.UserRoles
            .Select(ur => ur.Role.Name)
            .ToList();

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        var expirationMinutes = await GetExpirationMinutesAsync();
        var newToken = GenerateToken(user.Id, user.Email, user.Name, roles, permissions, expirationMinutes);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);

        return new RefreshTokenResponseDto
        {
            Token = newToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var email = request.Email.Trim();
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
        {
            return;
        }

        await _passwordResetTokenRepository.InvalidateActiveTokensAsync(user.Id, user.Id);

        var now = DateTimeOffset.UtcNow;
        var rawToken = GenerateResetToken();
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            ExpiresAt = now.AddMinutes(PasswordResetExpirationMinutes),
            UsedAt = null,
            CreatedAt = now,
            ModifiedAt = now,
            CreatedByUserId = user.Id,
            ModifiedByUserId = user.Id,
            IsDeleted = false
        };

        await _passwordResetTokenRepository.AddAsync(token);
        await _passwordResetTokenRepository.SaveChangesAsync();

        await _notificationService.SendPasswordResetCodeAsync(user.Email, user.Name, rawToken, PasswordResetExpirationMinutes);
        await _auditLogService.LogAsync(
            tableName: "PasswordResetTokens",
            action: "CREATE",
            newValues: new
            {
                token.UserId,
                token.ExpiresAt
            },
            primaryKey: token.Id.ToString(),
            affectedColumns: "UserId, TokenHash, ExpiresAt");
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new ValidationException("La nueva contrasena es obligatoria.");
            }

            var email = request.Email.Trim();
            var userByEmail = await _userRepository.GetByEmailAsync(email)
                ?? throw new ValidationException("Solicitud de recuperacion invalida o expirada.");

            var token = await _passwordResetTokenRepository.GetActiveByUserIdAsync(userByEmail.Id);
            if (token is null ||
                token.UsedAt.HasValue ||
                token.ExpiresAt < DateTimeOffset.UtcNow ||
                !string.Equals(token.TokenHash, HashToken(request.Token.Trim()), StringComparison.Ordinal))
            {
                throw new ValidationException("Solicitud de recuperacion invalida o expirada.");
            }

            var user = await _userRepository.GetByIdAsync(userByEmail.Id)
                ?? throw new ValidationException("Solicitud de recuperacion invalida o expirada.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ModifiedAt = DateTimeOffset.UtcNow;
            user.ModifiedByUserId = token.UserId;
            await _userRepository.SaveChangesAsync();

            var usedAt = DateTimeOffset.UtcNow;
            token.UsedAt = usedAt;
            token.ModifiedAt = usedAt;
            token.ModifiedByUserId = user.Id;
            await _passwordResetTokenRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                tableName: "Users",
                action: "RESET_PASSWORD",
                newValues: new { user.Id, Source = "PublicRecovery" },
                primaryKey: user.Id.ToString(),
                affectedColumns: "PasswordHash");

            return true;
        }
        catch (Exception ex) { return false; }
    }

    private string GenerateToken(int userId, string email, string name, List<string> roles, List<string> permissions, int expirationMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Name, name),
            new(ClaimTypes.Name, name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<int> GetExpirationMinutesAsync()
    {
        var param = await _parameterRepository.GetByNameAsync(ParameterNames.JwtExpirationMinutes);
        if (param is not null && int.TryParse(param.Value, out var minutes) && minutes > 0)
            return minutes;
        return _jwtSettings.ExpirationMinutes;
    }

    private static string GenerateResetToken()
    {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6");
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}

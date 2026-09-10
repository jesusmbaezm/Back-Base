namespace Services.DTOs.Auth;

public class RefreshTokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}

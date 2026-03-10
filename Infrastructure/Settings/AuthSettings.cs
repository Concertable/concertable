namespace Infrastructure.Settings;

public class AuthSettings
{
    public string? JwtSigningKeyBase64 { get; set; }
    public string Issuer { get; set; } = "Concertable";
    public string Audience { get; set; } = "Concertable";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}

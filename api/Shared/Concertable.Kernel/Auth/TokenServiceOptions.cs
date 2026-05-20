namespace Concertable.Shared.Infrastructure.Settings;

public class TokenServiceOptions
{
    public string Authority { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
}

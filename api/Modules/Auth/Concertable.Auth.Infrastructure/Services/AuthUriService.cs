namespace Concertable.Auth.Infrastructure.Services;

internal class AuthUriService : IAuthUriService
{
    private readonly IUriService _uriService;

    public AuthUriService(IUriService uriService)
    {
        _uriService = uriService;
    }

    public Uri GetEmailVerificationUri(string token) =>
        _uriService.GetUri("/verify-email", new Dictionary<string, string> { ["token"] = token });

    public Uri GetPasswordResetUri(string token) =>
        _uriService.GetUri("/reset-password", new Dictionary<string, string> { ["token"] = token });
}

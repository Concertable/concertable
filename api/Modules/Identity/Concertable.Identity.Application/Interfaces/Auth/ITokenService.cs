namespace Concertable.Identity.Application.Interfaces.Auth;

internal interface ITokenService
{
    string CreateAccessToken(Guid userId, string email, Role role);
    string CreateRefreshToken();
}

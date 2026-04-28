namespace Concertable.Auth.Application.Interfaces;

internal interface ITokenService
{
    string CreateAccessToken(Guid userId, string email, Role role);
    string CreateRefreshToken();
}

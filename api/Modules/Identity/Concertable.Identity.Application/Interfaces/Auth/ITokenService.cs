namespace Concertable.Application.Interfaces.Auth;

public interface ITokenService
{
    string CreateAccessToken(Guid userId, string email, Role role);
    string CreateRefreshToken();
}

using Core.Enums;

namespace Application.Interfaces.Auth;

public interface ITokenService
{
    string CreateAccessToken(int userId, string email, Role role);
    string CreateRefreshToken();
}

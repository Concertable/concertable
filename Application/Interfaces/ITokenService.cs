using Core.Enums;

namespace Application.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(int userId, string email, Role role);
    string CreateRefreshToken();
}

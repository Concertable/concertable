namespace Application.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(int userId, string email, string role);
    string CreateRefreshToken();
}

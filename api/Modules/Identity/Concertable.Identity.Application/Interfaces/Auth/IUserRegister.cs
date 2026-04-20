
namespace Concertable.Identity.Application.Interfaces.Auth;

public interface IUserRegister
{
    Task RegisterAsync(RegisterRequest request, string passwordHash);
}

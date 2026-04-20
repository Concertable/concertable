
namespace Concertable.Identity.Application.Interfaces.Auth;

internal interface IUserRegister
{
    Task RegisterAsync(RegisterRequest request, string passwordHash);
}

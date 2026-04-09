using Concertable.Application.Requests;

namespace Concertable.Application.Interfaces.Auth;

public interface IUserRegister
{
    Task RegisterAsync(RegisterRequest request, string passwordHash);
}

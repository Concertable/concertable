namespace Concertable.Identity.Application.Interfaces.Auth;

internal interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

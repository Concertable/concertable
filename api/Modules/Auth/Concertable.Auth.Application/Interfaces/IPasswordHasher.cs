namespace Concertable.Auth.Application.Interfaces;

internal interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

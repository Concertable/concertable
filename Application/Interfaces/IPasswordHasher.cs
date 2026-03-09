namespace Application.Interfaces;

/// <summary>
/// Hashes and verifies passwords for the new User entity (no Identity).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

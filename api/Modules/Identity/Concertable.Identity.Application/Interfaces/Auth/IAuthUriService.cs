namespace Concertable.Identity.Application.Interfaces.Auth;

public interface IAuthUriService
{
    Uri GetEmailVerificationUri(string token);
    Uri GetPasswordResetUri(string token);
}

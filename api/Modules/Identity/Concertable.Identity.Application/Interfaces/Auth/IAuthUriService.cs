namespace Concertable.Identity.Application.Interfaces.Auth;

internal interface IAuthUriService
{
    Uri GetEmailVerificationUri(string token);
    Uri GetPasswordResetUri(string token);
}

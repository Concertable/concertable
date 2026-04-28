namespace Concertable.Auth.Application.Interfaces;

internal interface IAuthUriService
{
    Uri GetEmailVerificationUri(string token);
    Uri GetPasswordResetUri(string token);
}

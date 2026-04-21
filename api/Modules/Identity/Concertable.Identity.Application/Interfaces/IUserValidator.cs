using FluentResults;

namespace Concertable.Identity.Application.Interfaces;

internal interface IUserValidator
{
    Task<Result> CanRegisterAsync(RegisterRequest request);
}

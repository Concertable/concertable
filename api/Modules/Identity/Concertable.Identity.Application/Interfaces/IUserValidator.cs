using FluentResults;

namespace Concertable.Identity.Application.Interfaces;

public interface IUserValidator
{
    Task<Result> CanRegisterAsync(RegisterRequest request);
}

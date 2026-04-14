using Concertable.Application.Requests;
using FluentResults;

namespace Concertable.Application.Interfaces;

public interface IUserValidator
{
    Task<Result> CanRegisterAsync(RegisterRequest request);
}

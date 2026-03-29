using Concertable.Application.Responses;
using Concertable.Application.Requests;

namespace Concertable.Application.Interfaces;

public interface IUserValidator
{
    Task<ValidationResult> CanRegisterAsync(RegisterRequest request);
}

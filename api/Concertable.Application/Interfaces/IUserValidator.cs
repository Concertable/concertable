using Application.Responses;
using Application.Requests;

namespace Application.Interfaces;

public interface IUserValidator
{
    Task<ValidationResult> CanRegisterAsync(RegisterRequest request);
}

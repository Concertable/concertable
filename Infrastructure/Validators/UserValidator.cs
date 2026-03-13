using Application.Interfaces;
using Application.Requests;
using Application.Responses;
using Core.Enums;

namespace Infrastructure.Validators;

public class UserValidator : IUserValidator
{
    private readonly IUserRepository userRepository;

    public UserValidator(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<ValidationResult> CanRegisterAsync(RegisterRequest request)
    {
        var result = new ValidationResult();

        if (request.Role == Role.Admin)
            result.AddError("You cannot make yourself an admin");

        if (await userRepository.ExistsByEmailAsync(request.Email))
            result.AddError("Email already exists");

        return result;
    }
}

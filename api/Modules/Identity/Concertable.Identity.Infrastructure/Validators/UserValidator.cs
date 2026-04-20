using Concertable.Application.Interfaces;
using Concertable.Application.Requests;
using FluentResults;

namespace Concertable.Infrastructure.Validators;

public class UserValidator : IUserValidator
{
    private readonly IUserRepository userRepository;

    public UserValidator(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<Result> CanRegisterAsync(RegisterRequest request)
    {
        var errors = new List<string>();

        if (request.Role == Role.Admin)
            errors.Add("You cannot make yourself an admin");

        if (await userRepository.ExistsByEmailAsync(request.Email))
            errors.Add("Email already exists");

        return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();
    }
}

namespace Concertable.Identity.Infrastructure;

internal class CustomerModule(IUserService userService, IUserMapper userMapper) : ICustomerModule
{
    public async Task<CustomerDto?> GetCustomerAsync(Guid userId)
    {
        var entity = await userService.GetUserEntityByIdAsync(userId);
        if (entity is not CustomerEntity customer) return null;
        return new CustomerDto
        {
            Id = customer.Id,
            Email = customer.Email ?? string.Empty,
            Role = customer.Role,
            IsEmailVerified = customer.IsEmailVerified
        };
    }
}

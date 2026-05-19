using Concertable.User.Infrastructure.Data;

namespace Concertable.User.Infrastructure.Services.Auth;

internal class VenueManagerRegister : IUserRegister
{
    private readonly UserDbContext context;

    public VenueManagerRegister(UserDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(string email, string passwordHash, Role role)
    {
        var user = UserEntity.Create(email, passwordHash, Role.VenueManager);
        context.Users.Add(user);
        context.VenueManagerProfiles.Add(new VenueManagerProfileEntity(user.Id));
        await context.SaveChangesAsync();
    }
}

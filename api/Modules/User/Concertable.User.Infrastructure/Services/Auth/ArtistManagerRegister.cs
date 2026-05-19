using Concertable.User.Infrastructure.Data;

namespace Concertable.User.Infrastructure.Services.Auth;

internal class ArtistManagerRegister : IUserRegister
{
    private readonly UserDbContext context;

    public ArtistManagerRegister(UserDbContext context)
    {
        this.context = context;
    }

    public async Task RegisterAsync(string email, string passwordHash, Role role)
    {
        var user = UserEntity.Create(email, passwordHash, Role.ArtistManager);
        context.Users.Add(user);
        context.ArtistManagerProfiles.Add(new ArtistManagerProfileEntity(user.Id));
        await context.SaveChangesAsync();
    }
}

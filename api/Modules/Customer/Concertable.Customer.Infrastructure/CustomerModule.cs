using Concertable.Customer.Application.Interfaces;
using Concertable.Customer.Contracts;
using Concertable.Identity.Contracts;

namespace Concertable.Customer.Infrastructure;

internal class CustomerModule : ICustomerModule
{
    private readonly IPreferenceService preferenceService;
    private readonly IIdentityModule identityModule;

    public CustomerModule(IPreferenceService preferenceService, IIdentityModule identityModule)
    {
        this.preferenceService = preferenceService;
        this.identityModule = identityModule;
    }

    public async Task<CustomerDto?> GetCustomerAsync(Guid userId)
    {
        var user = await identityModule.GetUserByIdAsync(userId);
        return user as CustomerDto;
    }

    public Task<IReadOnlyCollection<Guid>> GetUserIdsByLocationAndGenresAsync(
        double latitude,
        double longitude,
        IEnumerable<int> genreIds) =>
        preferenceService.GetUserIdsByLocationAndGenresAsync(latitude, longitude, genreIds);
}

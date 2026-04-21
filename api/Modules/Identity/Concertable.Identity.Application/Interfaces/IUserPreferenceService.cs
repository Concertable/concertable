namespace Concertable.Identity.Application.Interfaces;

internal interface IUserPreferenceService
{
    Task<IEnumerable<Guid>> GetUserIdsByPreferencesAsync();
}

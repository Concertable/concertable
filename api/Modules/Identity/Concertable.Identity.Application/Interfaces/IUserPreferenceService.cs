namespace Concertable.Identity.Application.Interfaces;

public interface IUserPreferenceService
{
    Task<IEnumerable<Guid>> GetUserIdsByPreferencesAsync();
}

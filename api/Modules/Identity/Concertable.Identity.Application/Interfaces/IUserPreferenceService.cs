namespace Concertable.Application.Interfaces;

public interface IUserPreferenceService
{
    Task<IEnumerable<Guid>> GetUserIdsByPreferencesAsync();
}

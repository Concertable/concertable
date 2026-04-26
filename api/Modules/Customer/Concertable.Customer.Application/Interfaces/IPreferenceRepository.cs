using Concertable.Application.Interfaces;

namespace Concertable.Customer.Application.Interfaces;

internal interface IPreferenceRepository : IIdRepository<PreferenceEntity>
{
    new Task<IEnumerable<PreferenceEntity>> GetAllAsync();

    new Task<PreferenceEntity?> GetByIdAsync(int id);

    Task<PreferenceEntity?> GetByUserIdAsync(Guid id);

    Task<IEnumerable<PreferenceEntity>> GetByMatchingGenresAsync(IEnumerable<int> genreIds);
}

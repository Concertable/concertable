using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IPreferenceRepository : IRepository<PreferenceEntity>
{
    new Task<IEnumerable<PreferenceEntity>> GetAllAsync();

    new Task<PreferenceEntity?> GetByIdAsync(int id);

    Task<PreferenceEntity?> GetByUserIdAsync(Guid id);
}

using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPreferenceRepository : IRepository<Preference>
    {
        new Task<IEnumerable<Preference>> GetAllAsync();

        new Task<Preference?> GetByIdAsync(int id);

        Task<Preference?> GetByUserIdAsync(int id);
    }
}

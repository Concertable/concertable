using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Interfaces;

public interface IUserPreferenceService
{
    Task<IEnumerable<Guid>> GetUserIdsByPreferencesAsync();
}

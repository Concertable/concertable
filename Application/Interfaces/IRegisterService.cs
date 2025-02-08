using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IRegisterService
    {
        Task<IEnumerable<Register>> GetAllForListingIdAsync(int listingId);
        Task RegisterForListingAsync(int listingId);
    }
}

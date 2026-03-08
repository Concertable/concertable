using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IManagerService
    {
        Task<int> GetIdByConcertIdAsync(int id);
        Task<int> GetIdByApplicationIdAsync(int id);
    }
}

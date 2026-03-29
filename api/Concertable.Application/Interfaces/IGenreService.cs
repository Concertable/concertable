using Concertable.Application.DTOs;
using Concertable.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Interfaces;

public interface IGenreService
{
    Task<IEnumerable<GenreDto>> GetAllAsync();
}

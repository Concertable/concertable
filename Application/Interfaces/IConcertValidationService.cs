using Application.DTOs;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IConcertValidationService
{
    Task<ValidationResponse> CanUpdateAsync(ConcertDto concertDto);
    Task<ValidationResponse> CanPostAsync(ConcertDto concertDto);
}

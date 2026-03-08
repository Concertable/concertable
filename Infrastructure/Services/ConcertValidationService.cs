using Application.DTOs;
using Application.Interfaces;
using Application.Responses;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class ConcertValidationService : IConcertValidationService
{
    private readonly IConcertRepository concertRepository;

    public ConcertValidationService(IConcertRepository concertRepository)
    {
        this.concertRepository = concertRepository;
    }

    public async Task<ValidationResponse> CanUpdateAsync(ConcertDto concertDto)
    {
        var concertEntity = await concertRepository.GetByIdAsync(concertDto.Id);

        if (concertEntity is null)
            return ValidationResponse.Failure("Concert not found");

        int ticketsSold = concertEntity.TotalTickets - concertEntity.AvailableTickets;

        if (concertDto.TotalTickets < ticketsSold)
            return ValidationResponse.Failure("Cannot reduce TotalTickets below the number of tickets already sold");

        return ValidationResponse.Success();
    }

    public Task<ValidationResponse> CanPostAsync(ConcertDto concertDto)
    {
        if (concertDto.DatePosted is not null)
            return Task.FromResult(ValidationResponse.Failure("You have already posted this concert"));

        return Task.FromResult(ValidationResponse.Success());
    }
}

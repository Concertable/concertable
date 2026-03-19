using Application.Interfaces;
using Application.Interfaces.Concert;
using Core.Entities;
using Core.Exceptions;

namespace Infrastructure.Services.Payment;

public class VenuePaymentRecipientResolver : IPaymentRecipientResolver
{
    private readonly IVenueManagerRepository venueManagerRepository;

    public VenuePaymentRecipientResolver(IVenueManagerRepository venueManagerRepository)
    {
        this.venueManagerRepository = venueManagerRepository;
    }

    public async Task<UserEntity> ResolveAsync(int concertId)
    {
        return await venueManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Venue manager not found for this concert");
    }
}

using Application.Interfaces;
using Application.Interfaces.Concert;
using Core.Entities;
using Core.Exceptions;

namespace Infrastructure.Services.Payment;

public class ArtistPaymentRecipientResolver : IPaymentRecipientResolver
{
    private readonly IArtistManagerRepository artistManagerRepository;

    public ArtistPaymentRecipientResolver(IArtistManagerRepository artistManagerRepository)
    {
        this.artistManagerRepository = artistManagerRepository;
    }

    public async Task<UserEntity> ResolveAsync(int concertId)
    {
        return await artistManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Artist manager not found for this concert");
    }
}

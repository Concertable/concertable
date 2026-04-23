using Concertable.Artist.Contracts.Events;
using Concertable.Concert.Domain;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Infrastructure.Handlers;

internal class ArtistReadModelProjectionHandler(ConcertDbContext db)
    : IIntegrationEventHandler<ArtistChangedEvent>
{
    public async Task HandleAsync(ArtistChangedEvent e, CancellationToken ct = default)
    {
        var artist = await db.ArtistReadModels
            .Include(a => a.Genres)
            .FirstOrDefaultAsync(a => a.Id == e.ArtistId, ct);

        if (artist is null)
        {
            artist = new ArtistReadModel
            {
                Id = e.ArtistId,
                UserId = e.UserId,
                Name = e.Name,
                Avatar = e.Avatar,
                BannerUrl = e.BannerUrl,
                County = e.County,
                Town = e.Town,
                Email = e.Email,
                Genres = e.GenreIds
                    .Select(id => new ArtistReadModelGenre { ArtistReadModelId = e.ArtistId, GenreId = id })
                    .ToList()
            };
            db.ArtistReadModels.Add(artist);
        }
        else
        {
            artist.UserId = e.UserId;
            artist.Name = e.Name;
            artist.Avatar = e.Avatar;
            artist.BannerUrl = e.BannerUrl;
            artist.County = e.County;
            artist.Town = e.Town;
            artist.Email = e.Email;

            var desired = e.GenreIds.ToHashSet();
            var current = artist.Genres.Select(g => g.GenreId).ToHashSet();

            foreach (var g in artist.Genres.Where(g => !desired.Contains(g.GenreId)).ToList())
                artist.Genres.Remove(g);

            foreach (var id in desired.Where(id => !current.Contains(id)))
                artist.Genres.Add(new ArtistReadModelGenre { ArtistReadModelId = e.ArtistId, GenreId = id });
        }

        await db.SaveChangesAsync(ct);
    }
}

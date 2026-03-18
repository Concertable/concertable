using Application.Interfaces.Concert;
using Concertable.Core.Entities.BookingContracts;
using Infrastructure.Data.Identity;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Concert;

public class BookingContractRepository : Repository<BookingContractEntity>, IBookingContractRepository
{
    public BookingContractRepository(ApplicationDbContext context) : base(context) { }

    public async Task<BookingContractEntity?> GetByOpportunityIdAsync(int opportunityId)
    {
        return await context.BookingContracts
            .Where(c => c.OpportunityId == opportunityId)
            .FirstOrDefaultAsync();
    }
}

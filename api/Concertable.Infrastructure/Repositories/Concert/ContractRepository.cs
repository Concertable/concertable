using Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Core.Enums;
using Infrastructure.Data.Identity;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Concert;

public class ContractRepository : Repository<ContractEntity>, IContractRepository
{
    public ContractRepository(ApplicationDbContext context) : base(context) { }

    public async Task<T?> GetByOpportunityIdAsync<T>(int opportunityId) where T : ContractEntity
    {
        return await context.ConcertOpportunities
            .Where(o => o.Id == opportunityId)
            .Select(o => o.Contract)
            .OfType<T>()
            .FirstOrDefaultAsync();
    }

    public async Task<T?> GetByConcertIdAsync<T>(int concertId) where T : ContractEntity
    {
        return await context.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => c.Application.Opportunity.Contract)
            .OfType<T>()
            .FirstOrDefaultAsync();
    }

    public async Task<T?> GetByApplicationIdAsync<T>(int applicationId) where T : ContractEntity
    {
        return await context.ConcertApplications
            .Where(a => a.Id == applicationId)
            .Select(a => a.Opportunity.Contract)
            .OfType<T>()
            .FirstOrDefaultAsync();
    }

    public async Task<ContractType?> GetTypeByConcertIdAsync(int concertId)
    {
        return await context.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => (ContractType?)c.Application.Opportunity.Contract.ContractType)
            .FirstOrDefaultAsync();
    }

    public async Task<ContractType?> GetTypeByApplicationIdAsync(int applicationId)
    {
        return await context.ConcertApplications
            .Where(a => a.Id == applicationId)
            .Select(a => (ContractType?)a.Opportunity.Contract.ContractType)
            .FirstOrDefaultAsync();
    }
}

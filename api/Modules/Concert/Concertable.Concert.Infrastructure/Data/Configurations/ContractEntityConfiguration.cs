using Concertable.Concert.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Concert.Infrastructure.Data.Configurations;

internal class ContractEntityConfiguration : IEntityTypeConfiguration<ContractEntity>
{
    public void Configure(EntityTypeBuilder<ContractEntity> builder)
    {
        builder.ToTable("Contracts");
        builder.UseTptMappingStrategy()
            .HasOne(c => c.Opportunity)
            .WithOne(o => o.Contract)
            .HasForeignKey<ContractEntity>(c => c.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class FlatFeeContractEntityConfiguration : IEntityTypeConfiguration<FlatFeeContractEntity>
{
    public void Configure(EntityTypeBuilder<FlatFeeContractEntity> builder)
        => builder.ToTable("FlatFeeContracts");
}

internal class DoorSplitContractEntityConfiguration : IEntityTypeConfiguration<DoorSplitContractEntity>
{
    public void Configure(EntityTypeBuilder<DoorSplitContractEntity> builder)
        => builder.ToTable("DoorSplitContracts");
}

internal class VersusContractEntityConfiguration : IEntityTypeConfiguration<VersusContractEntity>
{
    public void Configure(EntityTypeBuilder<VersusContractEntity> builder)
        => builder.ToTable("VersusContracts");
}

internal class VenueHireContractEntityConfiguration : IEntityTypeConfiguration<VenueHireContractEntity>
{
    public void Configure(EntityTypeBuilder<VenueHireContractEntity> builder)
        => builder.ToTable("VenueHireContracts");
}

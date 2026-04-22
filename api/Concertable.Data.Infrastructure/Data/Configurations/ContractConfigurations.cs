using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Data.Infrastructure.Data.Configurations;

public class ContractEntityConfiguration : IEntityTypeConfiguration<ContractEntity>
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

public class FlatFeeContractEntityConfiguration : IEntityTypeConfiguration<FlatFeeContractEntity>
{
    public void Configure(EntityTypeBuilder<FlatFeeContractEntity> builder)
        => builder.ToTable("FlatFeeContracts");
}

public class DoorSplitContractEntityConfiguration : IEntityTypeConfiguration<DoorSplitContractEntity>
{
    public void Configure(EntityTypeBuilder<DoorSplitContractEntity> builder)
        => builder.ToTable("DoorSplitContracts");
}

public class VersusContractEntityConfiguration : IEntityTypeConfiguration<VersusContractEntity>
{
    public void Configure(EntityTypeBuilder<VersusContractEntity> builder)
        => builder.ToTable("VersusContracts");
}

public class VenueHireContractEntityConfiguration : IEntityTypeConfiguration<VenueHireContractEntity>
{
    public void Configure(EntityTypeBuilder<VenueHireContractEntity> builder)
        => builder.ToTable("VenueHireContracts");
}

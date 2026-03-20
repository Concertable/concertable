using Application.Interfaces;
using Application.Interfaces.Concert;
using Core.Entities;
using Core.Enums;

namespace Application.Mappers;

public class ConcertApplicationMapper : IConcertApplicationMapper
{
    private readonly IContractStrategyFactory<IApplicationMapper> factory;

    public ConcertApplicationMapper(IContractStrategyFactory<IApplicationMapper> factory)
        => this.factory = factory;

    private ContractType GetContractType(ConcertApplicationEntity application) => application switch
    {
        FlatFeeApplicationEntity => ContractType.FlatFee,
        DoorSplitApplicationEntity => ContractType.DoorSplit,
        VersusApplicationEntity => ContractType.Versus,
        VenueHireApplicationEntity => ContractType.VenueHire,
        _ => throw new InvalidOperationException($"Unknown application type: {application.GetType().Name}")
    };

    public IConcertApplication ToDto(ConcertApplicationEntity application) =>
        factory.Create(GetContractType(application)).ToDto(application);

    public IConcertApplication ToArtistDto(ConcertApplicationEntity application) =>
        factory.Create(GetContractType(application)).ToArtistDto(application);

    public IEnumerable<IConcertApplication> ToDtos(IEnumerable<ConcertApplicationEntity> applications) =>
        applications.Select(ToDto);

    public IEnumerable<IConcertApplication> ToArtistDtos(IEnumerable<ConcertApplicationEntity> applications) =>
        applications.Select(ToArtistDto);
}

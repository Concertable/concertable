namespace Concertable.Core.Entities.BookingContracts;

public class VersusContractEntity : BookingContractEntity
{
    public decimal Guarantee { get; set; }
    public decimal ArtistDoorPercent { get; set; }
}

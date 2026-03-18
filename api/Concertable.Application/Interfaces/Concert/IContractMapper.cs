using Concertable.Core.Entities.BookingContracts;
using Core.Enums;

namespace Application.Interfaces.Concert;

public interface IContractMapper
{
    ContractType ContractType { get; }
    BookingContractEntity ToEntity(IBookingContract dto);
    IBookingContract ToDto(BookingContractEntity entity);
}

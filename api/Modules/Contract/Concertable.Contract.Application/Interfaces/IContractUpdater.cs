using Concertable.Contract.Abstractions;

namespace Concertable.Contract.Application.Interfaces;

internal interface IContractUpdater
{
    void Apply(ContractEntity existing, IContract source);
}

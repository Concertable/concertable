namespace Concertable.Concert.Domain;

internal interface ILifecycleEntity
{
    int Id { get; }
    ContractType ContractType { get; }
    ConcertStage CurrentStage { get; }
    void AdvanceStage(ConcertStage next);
}

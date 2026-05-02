namespace Concertable.Concert.Application.Interfaces;

// Every concert workflow must satisfy: some apply, some accept, settle, finish.
// Checkout is opt-in (not every contract has one).
internal interface IConcertWorkflow : IApplyable, IAcceptable, ISettleable, IFinishable { }

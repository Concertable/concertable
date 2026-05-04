namespace Concertable.Concert.Application.Interfaces;

// Every concert workflow must satisfy: some apply, some accept, some checkout, settle, finish.
// Phase of checkout (apply vs accept) is chosen by the family interface.
internal interface IConcertWorkflow : IApplyable, IAcceptable, ICheckoutable, ISettleable, IFinishable { }

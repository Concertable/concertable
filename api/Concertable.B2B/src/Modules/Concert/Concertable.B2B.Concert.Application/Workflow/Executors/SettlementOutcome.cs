namespace Concertable.B2B.Concert.Application.Workflow.Executors;

internal enum SettlementOutcome
{
    Settled,
    DeferredPendingPaymentTerms,
    DeferredPendingTaxCompliance,
    DeferredPendingSelfBillingAgreement,
}

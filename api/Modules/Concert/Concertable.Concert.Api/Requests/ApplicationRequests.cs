namespace Concertable.Concert.Api.Requests;

internal record ApplyWithPaymentMethodRequest(string PaymentMethodId);

internal record AcceptWithPaymentMethodRequest(string PaymentMethodId);

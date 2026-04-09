namespace Concertable.Application.Responses;

public record PaymentMethodResponse(string Brand, string Last4, int ExpMonth, int ExpYear);

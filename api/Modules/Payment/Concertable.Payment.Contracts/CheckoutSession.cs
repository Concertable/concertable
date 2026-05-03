namespace Concertable.Payment.Contracts;

public record CheckoutSession(string ClientSecret, string CustomerSession, string CustomerId, IntentType IntentType);

public enum IntentType { Payment, Setup }

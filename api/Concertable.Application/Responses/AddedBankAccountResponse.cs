namespace Concertable.Application.Responses;

public record AddedBankAccountResponse
{
    public required string AccountId { get; set; }
    public string? RedirectUri { get; set; }
}

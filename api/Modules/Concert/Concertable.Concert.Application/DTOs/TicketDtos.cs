namespace Concertable.Concert.Application.DTOs;

internal record UserDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public Role? Role { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? County { get; set; }
    public string? Town { get; set; }
}

internal record TicketConcertDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required string VenueName { get; set; }
    public required string ArtistName { get; set; }
}

internal record TicketDto
{
    public Guid Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public byte[] QrCode { get; set; } = null!;
    public required TicketConcertDto Concert { get; set; }
    public required UserDto User { get; set; }
}

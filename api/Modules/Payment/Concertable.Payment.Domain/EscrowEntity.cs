namespace Concertable.Payment.Domain;

public class EscrowEntity : IIdEntity, IAuditable
{
    private EscrowEntity() { }

    private EscrowEntity(int bookingId, Guid fromUserId, Guid toUserId, long amount, string chargeId, DateTime releaseAt)
    {
        BookingId = bookingId;
        FromUserId = fromUserId;
        ToUserId = toUserId;
        Amount = amount;
        ChargeId = chargeId;
        ReleaseAt = releaseAt;
        Status = EscrowStatus.Held;
    }

    public int Id { get; private set; }
    public int BookingId { get; private set; }
    public Guid FromUserId { get; private set; }
    public Guid ToUserId { get; private set; }
    public long Amount { get; private set; }
    public EscrowStatus Status { get; private set; }
    public string ChargeId { get; private set; } = null!;
    public string? TransferId { get; private set; }
    public string? RefundId { get; private set; }
    public DateTime ReleaseAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public static EscrowEntity Create(int bookingId, Guid fromUserId, Guid toUserId, long amount, string chargeId, DateTime releaseAt) =>
        new(bookingId, fromUserId, toUserId, amount, chargeId, releaseAt);

    public void Release(string transferId, DateTime now)
    {
        if (Status != EscrowStatus.Held)
            throw new DomainException("Only held escrow can be released.");
        TransferId = transferId;
        ReleasedAt = now;
        Status = EscrowStatus.Released;
    }

    public void Refund(string refundId, DateTime now)
    {
        if (Status is not (EscrowStatus.Held or EscrowStatus.Disputed))
            throw new DomainException("Only held or disputed escrow can be refunded.");
        RefundId = refundId;
        RefundedAt = now;
        Status = EscrowStatus.Refunded;
    }

    public void MarkDisputed()
    {
        if (Status != EscrowStatus.Held)
            throw new DomainException("Only held escrow can be disputed.");
        Status = EscrowStatus.Disputed;
    }
}

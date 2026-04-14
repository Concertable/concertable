using Concertable.Core.Exceptions;

namespace Concertable.Core.ValueObjects;

public record DateRange
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    public DateRange(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new DomainException("End must be after Start.");

        Start = start;
        End = end;
    }
}

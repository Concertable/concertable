using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces.Concert;

/// <summary>
/// Handles logic related to event scheduling and artist availability.
/// This validator is responsible for ensuring events do not conflict,
/// checking if artists are available on specific dates, and enforcing
/// scheduling constraints across the system.
/// </summary>
public interface IOpportunityApplicationValidator
{
    /// <summary>
    /// Checks whether an artist can apply for a given opportunity, based on conflicts and constraints.
    /// </summary>
    Task<ValidationResult> CanApplyAsync(int opportunityId, int artistId);

    /// <summary>
    /// Checks whether a venue manager can accept a concert application, based on availability.
    /// </summary>
    Task<ValidationResult> CanAcceptAsync(int applicationId);
}

using Application.Responses;

namespace Application.Interfaces;

/// <summary>
/// Handles logic related to event scheduling and artist availability.
/// This validator is responsible for ensuring events do not conflict,
/// checking if artists are available on specific dates, and enforcing
/// scheduling constraints across the system.
/// </summary>
public interface IListingApplicationValidator
{
    /// <summary>
    /// Checks whether an artist can apply for a given listing, based on conflicts and constraints.
    /// </summary>
    Task<ValidationResult> CanApplyForListingAsync(int listingId, int artistId);

    /// <summary>
    /// Checks whether a venue manager can accept a listing application, based on availability.
    /// </summary>
    Task<ValidationResult> CanAcceptListingApplicationAsync(int applicationId);
}

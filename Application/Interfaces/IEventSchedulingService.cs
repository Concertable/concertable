using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    /// <summary>
    /// Handles logic related to event scheduling and artist availability.
    /// This service is responsible for ensuring events do not conflict,
    /// checking if artists are available on specific dates, and enforcing
    /// scheduling constraints across the system.
    /// </summary>
    public interface IEventSchedulingService
    {
        /// <summary>
        /// Checks whether an artist can apply for a given listing, based on conflicts and constraints.
        /// </summary>
        Task<ValidationResponse> CanApplyForListingAsync(int listingId, int artistId);

        /// <summary>
        /// Checks whether a venue manager can accept a listing application, based on availability.
        /// </summary>
        Task<ValidationResponse> CanAcceptListingApplicationAsync(int applicationId);
    }
}

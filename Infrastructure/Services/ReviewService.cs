using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            this.reviewRepository = reviewRepository;
        }

        public async Task AddArtistRatingsAsync(IEnumerable<ArtistHeaderDto> headers)
        {
            foreach (var h in headers)
            {
                h.Rating = await reviewRepository.GetRatingByArtistIdAsync(h.Id);
            }
        }

        public async Task AddEventRatingsAsync(IEnumerable<EventHeaderDto> headers)
        {
            foreach (var h in headers)
            {
                h.Rating = await reviewRepository.GetRatingByEventIdAsync(h.Id);
            }
        }

        public async Task AddVenueRatingsAsync(IEnumerable<VenueHeaderDto> headers)
        {
            foreach (var h in headers)
            {
                h.Rating = await reviewRepository.GetRatingByVenueIdAsync(h.Id);
            }
        }
    }
}

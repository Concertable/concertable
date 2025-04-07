using Application.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Requests
{
    public class VenueCreateRequest
    {
        [Required]
        public CreateVenueDto Venue { get; set; }

        [Required(ErrorMessage = "An image is required.")]
        public IFormFile Image { get; set; }
    }
}

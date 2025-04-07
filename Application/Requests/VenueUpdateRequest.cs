using Application.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Requests
{
    public class VenueUpdateRequest
    {
        public VenueDto Venue { get; set; }
        public IFormFile Image { get; set; }
    }
}

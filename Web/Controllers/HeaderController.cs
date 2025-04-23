using Application.DTOs;
using Application.Interfaces;
using Application.Responses;
using Core.ModelBinders;
using Core.Parameters;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HeaderController : ControllerBase
    {
        private readonly IHeaderServiceFactory headerServiceFactory;

        public HeaderController(IHeaderServiceFactory headerServiceFactory)
        {
            this.headerServiceFactory = headerServiceFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Get([ModelBinder(BinderType = typeof(SearchParamsModelBinder))][FromQuery] SearchParams searchParams)
        {
            if (string.IsNullOrWhiteSpace(searchParams.HeaderType))
                return BadRequest("Header type is required");

            try
            {
                object response = searchParams.HeaderType.ToLower() switch
                {
                    "venue" => await headerServiceFactory.GetService<VenueHeaderDto>(searchParams.HeaderType)
                        .GetHeadersAsync(searchParams),

                    "artist" => await headerServiceFactory.GetService<ArtistHeaderDto>(searchParams.HeaderType)
                        .GetHeadersAsync(searchParams),

                    "event" => await headerServiceFactory.GetService<EventHeaderDto>(searchParams.HeaderType)
                        .GetHeadersAsync(searchParams),

                    _ => throw new ArgumentException($"Invalid entity type '{searchParams.HeaderType}'.")
                };
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

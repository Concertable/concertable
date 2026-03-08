using Application.Interfaces.Search;
using Core.ModelBinders;
using Core.Parameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class HeaderController : ControllerBase
    {
        private readonly IHeaderServiceFactory headerServiceFactory;

        public HeaderController(IHeaderServiceFactory headerServiceFactory)
        {
            this.headerServiceFactory = headerServiceFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [ModelBinder(BinderType = typeof(SearchParamsModelBinder))][FromQuery] SearchParams searchParams)
        {
            if (string.IsNullOrWhiteSpace(searchParams.HeaderType))
                return BadRequest("Search type is required.");

            var service = headerServiceFactory.Create(searchParams.HeaderType);

            if (service is null)
                return BadRequest($"Invalid header type '{searchParams.HeaderType}'.");

            return Ok(await service.SearchAsync(searchParams));
        }

        [HttpGet("amount/{amount}")]
        public async Task<IActionResult> GetByAmount(int amount, [FromQuery] string headerType)
        {
            if (string.IsNullOrWhiteSpace(headerType))
                return BadRequest("Header type is required.");

            var service = headerServiceFactory.Create(headerType);

            if (service is null)
                return BadRequest($"Invalid header type '{headerType}'.");

            return Ok(await service.GetByAmountAsync(amount));
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopular([FromQuery] string headerType)
        {
            if (string.IsNullOrWhiteSpace(headerType))
                return BadRequest("Header type is required.");

            var service = headerServiceFactory.Create(headerType);

            if (service is null)
                return BadRequest($"Invalid header type '{headerType}'.");

            return Ok(await service.GetPopularAsync());
        }

        [HttpGet("free")]
        public async Task<IActionResult> GetFree([FromQuery] string headerType)
        {
            if (string.IsNullOrWhiteSpace(headerType))
                return BadRequest("Header type is required.");

            var service = headerServiceFactory.Create(headerType);

            if (service is null)
                return BadRequest($"Invalid header type '{headerType}'.");

            return Ok(await service.GetFreeAsync());
        }
    }
}

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
    public class SearchController : ControllerBase
    {
        private readonly ISearchServiceFactory searchServiceFactory;

        public SearchController(ISearchServiceFactory searchServiceFactory)
        {
            this.searchServiceFactory = searchServiceFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [ModelBinder(BinderType = typeof(SearchParamsModelBinder))][FromQuery] SearchParams searchParams)
        {
            if (string.IsNullOrWhiteSpace(searchParams.HeaderType))
                return BadRequest("Search type is required.");

            var service = searchServiceFactory.Create(searchParams.HeaderType);

            if (service is null)
                return BadRequest($"Invalid search type '{searchParams.HeaderType}'.");

            return Ok(await service.SearchAsync(searchParams));
        }
    }
}

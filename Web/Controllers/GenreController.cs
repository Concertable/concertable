using Core.Entities;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService genreService;

        public GenreController(IGenreService genreService)
        {
            this.genreService = genreService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Genre>>> GetAll()
        {
            return Ok(await genreService.GetAllAsync());
        }
    }
}

using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Concertable.Application.DTOs;

namespace Concertable.Web.Controllers;

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
    public async Task<ActionResult<IEnumerable<GenreDto>>> GetAll()
    {
        return Ok(await genreService.GetAllAsync());
    }
}

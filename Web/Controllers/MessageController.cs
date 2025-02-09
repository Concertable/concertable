using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Parameters;
using Core.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService messageService;

        public MessageController(IMessageService messageService)
        {
            this.messageService = messageService;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<MessageSummaryDto>> GetSummaryForUser([FromQuery] PaginationParams? pageParams)
        {
            if (pageParams == null)
            {
                pageParams = new PaginationParams { PageNumber = 1, PageSize = 5 };
            }

            pageParams.PageNumber = 1;
            pageParams.PageSize = 5;
            return Ok(await messageService.GetSummaryForUser(pageParams));
        }

        [HttpGet("all")]
        public async Task<ActionResult<PaginationResponse<Message>>> GetAllForUser([FromQuery] PaginationParams pageParams)
        {
            return Ok(await messageService.GetAllForUserAsync(pageParams));
        }

        private PaginationParams EnsurePaginationParams(PaginationParams? pageParams)
        {
            if (pageParams == null)
            {
                return new PaginationParams { PageNumber = 1, PageSize = 5 };
            }

            pageParams.PageSize = Math.Min(pageParams.PageSize, 5);
            return pageParams;
        }
    }
}

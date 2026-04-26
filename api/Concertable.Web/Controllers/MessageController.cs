using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Concertable.Application.Requests;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessageService messageService;

    public MessageController(IMessageService messageService)
    {
        this.messageService = messageService;
    }

    [HttpGet("user/summary")]
    public async Task<ActionResult<MessageSummaryDto>> GetSummaryForUser()
    {
        return Ok(await messageService.GetSummaryForUser());
    }

    [HttpGet("user")]
    public async Task<ActionResult<Pagination<MessageEntity>>> GetForUser([FromQuery] PageParams pageParams)
    {
        return Ok(await messageService.GetForUserAsync(pageParams));
    }

    [HttpGet("user/unread-count")]
    public async Task<ActionResult<int>> GetUnreadCountForUser()
    {
        return Ok(await messageService.GetUnreadCountForUserAsync());
    }

    [HttpPost("mark-read")]
    public async Task<ActionResult<int>> MarkAsReadAsync([FromBody] MarkMessagesReadRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        await messageService.MarkAsReadAsync(request.MessageIds);

        var unreadCount = await messageService.GetUnreadCountForUserAsync();
        return Ok(unreadCount);
    }
}

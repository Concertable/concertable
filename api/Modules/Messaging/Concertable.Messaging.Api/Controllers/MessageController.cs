using Concertable.Core.Parameters;
using Concertable.Messaging.Application.DTOs;
using Concertable.Messaging.Application.Interfaces;
using Concertable.Messaging.Application.Requests;
using Concertable.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Messaging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class MessageController : ControllerBase
{
    private readonly IMessageService messageService;

    public MessageController(IMessageService messageService)
    {
        this.messageService = messageService;
    }

    [HttpGet("user/summary")]
    public async Task<ActionResult<MessageSummaryDto>> GetSummaryForUser() =>
        Ok(await messageService.GetSummaryForUser());

    [HttpGet("user")]
    public async Task<ActionResult<IPagination<MessageDto>>> GetForUser([FromQuery] PageParams pageParams) =>
        Ok(await messageService.GetForUserAsync(pageParams));

    [HttpGet("user/unread-count")]
    public async Task<ActionResult<int>> GetUnreadCountForUser() =>
        Ok(await messageService.GetUnreadCountForUserAsync());

    [HttpPost("mark-read")]
    public async Task<ActionResult<int>> MarkAsRead([FromBody] MarkMessagesReadRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        await messageService.MarkAsReadAsync(request.MessageIds);
        var unreadCount = await messageService.GetUnreadCountForUserAsync();
        return Ok(unreadCount);
    }
}

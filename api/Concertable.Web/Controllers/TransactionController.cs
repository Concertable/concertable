using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Results;
using Concertable.Core.Parameters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Concertable.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService purchaseService;

    public TransactionController(ITransactionService purchaseService)
    {
        this.purchaseService = purchaseService;
    }

    [HttpGet]
    public async Task<ActionResult<Pagination<ITransaction>>> GetPurchases([FromQuery] PageParams pageParams)
    {
        var result = await purchaseService.GetAsync(pageParams);
        return Ok(result);
    }
}

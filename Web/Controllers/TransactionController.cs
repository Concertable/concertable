using Application.DTOs;
using Application.Interfaces;
using Application.Responses;
using Core.Parameters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Web.Controllers
{
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
        public async Task<ActionResult<PaginationResponse<TransactionDto>>> GetPurchases([FromQuery] PaginationParams pageParams)
        {
            var result = await purchaseService.GetAsync(pageParams);
            return Ok(result);
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace Concertable.Concert.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class ContractController : ControllerBase
{
    private readonly IContractService contractService;

    public ContractController(IContractService contractService)
    {
        this.contractService = contractService;
    }

    [HttpGet("opportunity/{opportunityId}")]
    public async Task<IActionResult> GetByOpportunityId(int opportunityId)
    {
        return Ok(await contractService.GetByOpportunityIdAsync(opportunityId));
    }
}

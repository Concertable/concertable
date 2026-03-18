using Application.Interfaces.Concert;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractController : ControllerBase
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

    [Authorize(Roles = "VenueManager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] IContract contract)
    {
        await contractService.CreateAsync(contract);
        return Created();
    }
}

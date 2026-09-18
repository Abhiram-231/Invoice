using Billing.Application.Interfaces;
using Billing.Contracts;
using Billing.Contracts.Financial;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/financial")]
[Consumes("application/json")]
public class FinancialController : ControllerBase
{
    private readonly IFinancialCalculationEngine _financialCalculationEngine;

    public FinancialController(IFinancialCalculationEngine financialCalculationEngine)
    {
        _financialCalculationEngine = financialCalculationEngine;
    }

    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Calculate([FromBody] FinancialCalculationRequest request)
    {
        var tenantId = GetTenantId();
        if (!tenantId.HasValue) return Forbid();

        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                       ?? User.FindFirst("role")?.Value;
        var userName = User.Identity?.Name
                       ?? User.FindFirst("name")?.Value
                       ?? "System";

        var result = await _financialCalculationEngine.CalculateAsync(request, tenantId.Value, userRole, userName);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    private int? GetTenantId()
    {
        var claim = User.FindFirst("TenantId")?.Value ?? User.FindFirst("tenant_id")?.Value;
        if (int.TryParse(claim, out var tid) && tid > 0) return tid;

        if (User.IsInRole("SuperAdmin"))
        {
            if (Request.Headers.TryGetValue("X-Tenant-Id", out var h) && int.TryParse(h, out var hId) && hId > 0)
                return hId;
            return 1;
        }

        return null;
    }
}

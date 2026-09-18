using System.Security.Claims;
using Billing.Application.Interfaces;
using Billing.Contracts;
using Billing.Contracts.Discount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Billing.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/discounts")]
[Consumes("application/json")]
[Produces("application/json")]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountService _discountService;

    public DiscountsController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    /// <summary>
    /// IBMSBE-007: Calculate line-level item discount.
    /// </summary>
    [HttpPost("calculate-line")]
    [ProducesResponseType(typeof(LineDiscountResultDto), StatusCodes.Status200OK)]
    public IActionResult CalculateLineDiscount([FromBody] CalculateLineDiscountRequest request)
    {
        var result = _discountService.CalculateLineDiscount(request);
        return Ok(result);
    }

    /// <summary>
    /// IBMSBE-007..010: Calculate invoice discounts with limits, role checks, and override auditing.
    /// </summary>
    [HttpPost("calculate-invoice")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDiscountResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDiscountResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateInvoiceDiscount(
        [FromBody] CalculateInvoiceDiscountRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var userRole = GetUserRole();
        var userName = GetUserName();

        var result = await _discountService.CalculateInvoiceDiscountAsync(
            request,
            tenantId,
            userRole,
            userName,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// IBMSBE-006: Retrieve paged discount rules for current tenant.
    /// </summary>
    [HttpGet("rules")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DiscountRuleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRules(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _discountService.GetRulesAsync(tenantId, page, pageSize, activeOnly, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// IBMSBE-006: Retrieve discount rule by ID.
    /// </summary>
    [HttpGet("rules/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DiscountRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DiscountRuleDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRuleById(int id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _discountService.GetRuleByIdAsync(id, tenantId, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// IBMSBE-009: Create discount rule (Manager, Admin, TenantAdmin, SuperAdmin).
    /// </summary>
    [HttpPost("rules")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin,Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<DiscountRuleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DiscountRuleDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRule(
        [FromBody] CreateDiscountRuleRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var userRole = GetUserRole();

        var result = await _discountService.CreateRuleAsync(request, tenantId, userRole, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetRuleById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// IBMSBE-009: Update discount rule.
    /// </summary>
    [HttpPut("rules/{id:int}")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin,Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<DiscountRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DiscountRuleDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRule(
        int id,
        [FromBody] UpdateDiscountRuleRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var userRole = GetUserRole();

        var result = await _discountService.UpdateRuleAsync(id, request, tenantId, userRole, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete discount rule.
    /// </summary>
    [HttpDelete("rules/{id:int}")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRule(int id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var userRole = GetUserRole();

        var result = await _discountService.DeleteRuleAsync(id, tenantId, userRole, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    private int GetTenantId()
    {
        var claim = User.FindFirst("TenantId")?.Value ?? User.FindFirst("tenant_id")?.Value;
        if (int.TryParse(claim, out var tId) && tId > 0)
            return tId;

        if (Request.Headers.TryGetValue("X-Tenant-Id", out var hVal) && int.TryParse(hVal, out var hId) && hId > 0)
            return hId;

        return 1;
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value
               ?? User.FindFirst("role")?.Value
               ?? User.FindFirst("Roles")?.Value
               ?? "Cashier";
    }

    private string GetUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value
               ?? User.FindFirst("name")?.Value
               ?? User.FindFirst(ClaimTypes.Email)?.Value
               ?? "Authorized User";
    }
}
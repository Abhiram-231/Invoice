using Billing.Application.Interfaces;
using Billing.Contracts;
using Billing.Contracts.Numbering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/settings/numbering")]
[Consumes("application/json")]
public class NumberingSettingsController : ControllerBase
{
    private readonly INumberingSettingService _numberingSettingService;
    private readonly INumberGenerationService _numberGenerationService;

    public NumberingSettingsController(
        INumberingSettingService numberingSettingService,
        INumberGenerationService numberGenerationService)
    {
        _numberingSettingService = numberingSettingService;
        _numberGenerationService = numberGenerationService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNumberingSettings([FromQuery] string? documentType = null)
    {
        var tenantId = GetTenantId();
        if (!tenantId.HasValue) return Forbid();

        if (!string.IsNullOrWhiteSpace(documentType))
        {
            var result = await _numberingSettingService.GetSettingByDocTypeAsync(documentType, tenantId.Value);
            return Ok(result);
        }

        var allResult = await _numberingSettingService.GetAllSettingsAsync(tenantId.Value);
        return Ok(allResult);
    }

    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateNumberingSettings([FromBody] UpdateNumberingSettingRequest request)
    {
        var tenantId = GetTenantId();
        if (!tenantId.HasValue) return Forbid();

        var result = await _numberingSettingService.UpdateSettingAsync(request, tenantId.Value);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult PreviewNumber([FromBody] NumberPreviewRequest request)
    {
        var preview = _numberingSettingService.Preview(request ?? new NumberPreviewRequest());
        return Ok(ApiResponse<NumberPreviewResponseDto>.Ok(preview, "Number preview generated."));
    }

    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateNextNumber([FromBody] GenerateNumberRequest request)
    {
        var tenantId = GetTenantId();
        if (!tenantId.HasValue) return Forbid();

        var result = await _numberGenerationService.GenerateNextNumberAsync(request, tenantId.Value);
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

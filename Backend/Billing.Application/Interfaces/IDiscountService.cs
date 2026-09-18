using Billing.Contracts;
using Billing.Contracts.Discount;

namespace Billing.Application.Interfaces;

public interface IDiscountService
{
    LineDiscountResultDto CalculateLineDiscount(CalculateLineDiscountRequest request);

    Task<ApiResponse<InvoiceDiscountResultDto>> CalculateInvoiceDiscountAsync(
        CalculateInvoiceDiscountRequest request,
        int tenantId,
        string? userRole = null,
        string? userName = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<DiscountRuleDto>> CreateRuleAsync(
        CreateDiscountRuleRequest request,
        int tenantId,
        string? userRole = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<DiscountRuleDto>> UpdateRuleAsync(
        int id,
        UpdateDiscountRuleRequest request,
        int tenantId,
        string? userRole = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<DiscountRuleDto>> GetRuleByIdAsync(
        int id,
        int tenantId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResult<DiscountRuleDto>>> GetRulesAsync(
        int tenantId,
        int page = 1,
        int pageSize = 10,
        bool? activeOnly = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteRuleAsync(
        int id,
        int tenantId,
        string? userRole = null,
        CancellationToken cancellationToken = default);
}
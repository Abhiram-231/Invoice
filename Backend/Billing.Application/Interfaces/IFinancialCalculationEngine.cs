using Billing.Contracts;
using Billing.Contracts.Financial;

namespace Billing.Application.Interfaces;

public interface IFinancialCalculationEngine
{
    Task<ApiResponse<FinancialCalculationResultDto>> CalculateAsync(FinancialCalculationRequest request, int tenantId, string? userRole = null, string? userName = null);
}

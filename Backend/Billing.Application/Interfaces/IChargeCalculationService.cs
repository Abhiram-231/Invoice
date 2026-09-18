using Billing.Contracts;
using Billing.Contracts.Charges;
using Billing.Domain.Entities;

namespace Billing.Application.Interfaces;

public interface IChargeCalculationService
{
    Task<ApiResponse<ChargeCalculationResultDto>> CalculateChargesAsync(CalculateChargesRequest request, int tenantId);
    ChargeCalculationResultDto Calculate(decimal subtotal, IEnumerable<ChargeConfiguration> charges);
}

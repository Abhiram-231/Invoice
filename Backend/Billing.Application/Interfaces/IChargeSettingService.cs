using Billing.Contracts;
using Billing.Contracts.Charges;

namespace Billing.Application.Interfaces;

public interface IChargeSettingService
{
    Task<ApiResponse<List<ChargeConfigurationDto>>> GetChargesAsync(int tenantId, bool? activeOnly = null, string? chargeType = null);
    Task<ApiResponse<ChargeConfigurationDto>> GetChargeByIdAsync(int id, int tenantId);
    Task<ApiResponse<ChargeConfigurationDto>> CreateChargeAsync(CreateChargeRequest request, int tenantId);
    Task<ApiResponse<ChargeConfigurationDto>> UpdateChargeAsync(int id, UpdateChargeRequest request, int tenantId);
    Task<ApiResponse<bool>> DeleteChargeAsync(int id, int tenantId);
}

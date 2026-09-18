using Billing.Contracts;
using Billing.Contracts.Tax;

namespace Billing.Application.Interfaces;

public interface ITaxSettingService
{
    Task<ApiResponse<TaxSettingsDto>> GetTaxSettingsAsync(
        int tenantId,
        string? taxType = null,
        string? status = null,
        bool? isActive = null,
        string? applicationLevel = null);

    Task<ApiResponse<TaxSettingsDto>> UpdateTaxSettingsAsync(
        UpdateTaxSettingsRequest request,
        int tenantId);

    Task<ApiResponse<TaxRateDto>> GetTaxRateByIdAsync(int id, int tenantId);

    Task<ApiResponse<TaxRateDto>> CreateTaxRateAsync(CreateTaxRateRequest request, int tenantId);

    Task<ApiResponse<TaxRateDto>> UpdateTaxRateAsync(int id, UpdateTaxRateRequest request, int tenantId);

    Task<ApiResponse<bool>> DeleteTaxRateAsync(int id, int tenantId);
}

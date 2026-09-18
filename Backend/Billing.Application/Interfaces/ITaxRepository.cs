using Billing.Domain.Entities;

namespace Billing.Application.Interfaces;

public interface ITaxRepository
{
    Task<TaxSetting?> GetSettingsAsync(int tenantId);
    Task<TaxSetting> UpsertSettingsAsync(TaxSetting settings);

    Task<TaxRate?> GetRateByIdAsync(int id, int tenantId);
    Task<TaxRate?> GetRateByCodeAsync(string code, int tenantId);
    Task<List<TaxRate>> GetRatesAsync(int tenantId, string? taxType = null, string? status = null, bool? isActive = null, string? applicationLevel = null);
    Task<List<TaxRate>> GetRatesByIdsAsync(IEnumerable<int> ids, int tenantId);
    Task<List<TaxRate>> GetRatesByCodesAsync(IEnumerable<string> codes, int tenantId);
    Task<TaxRate> CreateRateAsync(TaxRate taxRate);
    Task<TaxRate> UpdateRateAsync(TaxRate taxRate);
    Task<bool> DeleteRateAsync(int id, int tenantId);
    Task<bool> ExistsByCodeAsync(string code, int tenantId, int? excludeId = null);
}

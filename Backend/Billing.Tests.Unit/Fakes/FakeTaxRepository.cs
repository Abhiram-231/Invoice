using Billing.Application.Interfaces;
using Billing.Domain.Entities;

namespace Billing.Tests.Unit.Fakes;

public class FakeTaxRepository : ITaxRepository
{
    public List<TaxSetting> Settings { get; } = new();
    public List<TaxRate> Rates { get; } = new();
    private int _nextSettingId = 1;
    private int _nextRateId = 1;

    public Task<TaxSetting?> GetSettingsAsync(int tenantId)
    {
        var setting = Settings.FirstOrDefault(s => s.TenantId == tenantId);
        return Task.FromResult(setting);
    }

    public Task<TaxSetting> UpsertSettingsAsync(TaxSetting settings)
    {
        var existing = Settings.FirstOrDefault(s => s.TenantId == settings.TenantId);
        if (existing == null)
        {
            settings.Id = _nextSettingId++;
            settings.CreatedAtUtc = DateTime.UtcNow;
            Settings.Add(settings);
            return Task.FromResult(settings);
        }

        existing.IsTaxEnabled = settings.IsTaxEnabled;
        existing.DefaultTaxCalculation = settings.DefaultTaxCalculation;
        existing.PricesIncludeTax = settings.PricesIncludeTax;
        existing.DefaultTaxRateId = settings.DefaultTaxRateId;
        existing.TaxRegistrationNumber = settings.TaxRegistrationNumber;
        existing.TaxNumberLabel = settings.TaxNumberLabel;
        existing.EnableMultipleTaxes = settings.EnableMultipleTaxes;
        existing.State = settings.State;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        return Task.FromResult(existing);
    }

    public Task<TaxRate?> GetRateByIdAsync(int id, int tenantId)
    {
        var rate = Rates.FirstOrDefault(r => r.Id == id && r.TenantId == tenantId);
        return Task.FromResult(rate);
    }

    public Task<TaxRate?> GetRateByCodeAsync(string code, int tenantId)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var rate = Rates.FirstOrDefault(r => r.TenantId == tenantId && r.Code.ToUpper() == normalized);
        return Task.FromResult(rate);
    }

    public Task<List<TaxRate>> GetRatesAsync(
        int tenantId,
        string? taxType = null,
        string? status = null,
        bool? isActive = null,
        string? applicationLevel = null)
    {
        var query = Rates.Where(r => r.TenantId == tenantId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(taxType) && !string.Equals(taxType, "all", StringComparison.OrdinalIgnoreCase))
        {
            var t = taxType.Trim().ToUpperInvariant();
            query = query.Where(r => r.TaxType.ToUpper() == t);
        }

        if (isActive.HasValue)
        {
            var targetStatus = isActive.Value ? "Active" : "Inactive";
            query = query.Where(r => r.Status == targetStatus);
        }
        else if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(r => string.Equals(r.Status, status.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(applicationLevel) && !string.Equals(applicationLevel, "all", StringComparison.OrdinalIgnoreCase))
        {
            var level = applicationLevel.Trim();
            query = query.Where(r => r.ApplicationLevel.Equals(level, StringComparison.OrdinalIgnoreCase) ||
                                     r.ApplicationLevel.Equals("Both", StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(query.OrderBy(r => r.Priority).ThenBy(r => r.Name).ToList());
    }

    public Task<List<TaxRate>> GetRatesByIdsAsync(IEnumerable<int> ids, int tenantId)
    {
        var idList = ids.Distinct().ToList();
        var list = Rates
            .Where(r => r.TenantId == tenantId && idList.Contains(r.Id))
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<List<TaxRate>> GetRatesByCodesAsync(IEnumerable<string> codes, int tenantId)
    {
        var codeList = codes.Select(c => c.Trim().ToUpperInvariant()).Distinct().ToList();
        var list = Rates
            .Where(r => r.TenantId == tenantId && codeList.Contains(r.Code.ToUpper()))
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<TaxRate> CreateRateAsync(TaxRate taxRate)
    {
        taxRate.Id = _nextRateId++;
        taxRate.CreatedAtUtc = DateTime.UtcNow;
        Rates.Add(taxRate);
        return Task.FromResult(taxRate);
    }

    public Task<TaxRate> UpdateRateAsync(TaxRate taxRate)
    {
        var index = Rates.FindIndex(r => r.Id == taxRate.Id && r.TenantId == taxRate.TenantId);
        if (index >= 0)
        {
            taxRate.UpdatedAtUtc = DateTime.UtcNow;
            Rates[index] = taxRate;
        }
        return Task.FromResult(taxRate);
    }

    public Task<bool> DeleteRateAsync(int id, int tenantId)
    {
        var count = Rates.RemoveAll(r => r.Id == id && r.TenantId == tenantId);
        return Task.FromResult(count > 0);
    }

    public Task<bool> ExistsByCodeAsync(string code, int tenantId, int? excludeId = null)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var exists = Rates.Any(r =>
            r.TenantId == tenantId &&
            r.Code.ToUpper() == normalized &&
            (!excludeId.HasValue || r.Id != excludeId.Value));
        return Task.FromResult(exists);
    }
}

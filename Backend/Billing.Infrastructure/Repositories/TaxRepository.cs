using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Repositories;

public class TaxRepository : ITaxRepository
{
    private readonly BillingDbContext _context;

    public TaxRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<TaxSetting?> GetSettingsAsync(int tenantId)
    {
        return await _context.TaxSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId);
    }

    public async Task<TaxSetting> UpsertSettingsAsync(TaxSetting settings)
    {
        var existing = await _context.TaxSettings
            .FirstOrDefaultAsync(s => s.TenantId == settings.TenantId);

        if (existing == null)
        {
            settings.CreatedAtUtc = DateTime.UtcNow;
            settings.RowVersion = DateTime.UtcNow;
            _context.TaxSettings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
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
        existing.RowVersion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<TaxRate?> GetRateByIdAsync(int id, int tenantId)
    {
        return await _context.TaxRates
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);
    }

    public async Task<TaxRate?> GetRateByCodeAsync(string code, int tenantId)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await _context.TaxRates
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Code.ToUpper() == normalized);
    }

    public async Task<List<TaxRate>> GetRatesAsync(
        int tenantId,
        string? taxType = null,
        string? status = null,
        bool? isActive = null,
        string? applicationLevel = null)
    {
        var query = _context.TaxRates.Where(r => r.TenantId == tenantId);

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
            query = query.Where(r => r.Status == status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(applicationLevel) && !string.Equals(applicationLevel, "all", StringComparison.OrdinalIgnoreCase))
        {
            var level = applicationLevel.Trim();
            query = query.Where(r => r.ApplicationLevel == level || r.ApplicationLevel == "Both");
        }

        return await query
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<TaxRate>> GetRatesByIdsAsync(IEnumerable<int> ids, int tenantId)
    {
        var idList = ids.Distinct().ToList();
        return await _context.TaxRates
            .Where(r => r.TenantId == tenantId && idList.Contains(r.Id))
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<TaxRate>> GetRatesByCodesAsync(IEnumerable<string> codes, int tenantId)
    {
        var codeList = codes.Select(c => c.Trim().ToUpperInvariant()).Distinct().ToList();
        return await _context.TaxRates
            .Where(r => r.TenantId == tenantId && codeList.Contains(r.Code.ToUpper()))
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<TaxRate> CreateRateAsync(TaxRate taxRate)
    {
        taxRate.CreatedAtUtc = DateTime.UtcNow;
        taxRate.RowVersion = DateTime.UtcNow;
        _context.TaxRates.Add(taxRate);
        await _context.SaveChangesAsync();
        return taxRate;
    }

    public async Task<TaxRate> UpdateRateAsync(TaxRate taxRate)
    {
        taxRate.UpdatedAtUtc = DateTime.UtcNow;
        taxRate.RowVersion = DateTime.UtcNow;
        _context.TaxRates.Update(taxRate);
        await _context.SaveChangesAsync();
        return taxRate;
    }

    public async Task<bool> DeleteRateAsync(int id, int tenantId)
    {
        var existing = await GetRateByIdAsync(id, tenantId);
        if (existing == null) return false;

        _context.TaxRates.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByCodeAsync(string code, int tenantId, int? excludeId = null)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await _context.TaxRates.AnyAsync(r =>
            r.TenantId == tenantId &&
            r.Code.ToUpper() == normalized &&
            (!excludeId.HasValue || r.Id != excludeId.Value));
    }
}

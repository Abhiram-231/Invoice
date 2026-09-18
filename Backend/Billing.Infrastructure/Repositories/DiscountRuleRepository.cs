using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Repositories;

public class DiscountRuleRepository : IDiscountRuleRepository
{
    private readonly BillingDbContext _context;

    public DiscountRuleRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<DiscountRule?> GetByIdAsync(int id, int tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountRules
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, cancellationToken);
    }

    public async Task<DiscountRule?> GetByCodeAsync(string code, int tenantId, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await _context.DiscountRules
            .FirstOrDefaultAsync(r => r.Code == normalized && r.TenantId == tenantId, cancellationToken);
    }

    public async Task<(List<DiscountRule> Items, int TotalCount)> GetPagedAsync(
        int tenantId,
        int page,
        int pageSize,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DiscountRules
            .Where(r => r.TenantId == tenantId);

        if (activeOnly.HasValue)
        {
            query = activeOnly.Value
                ? query.Where(r => r.Status == "Active")
                : query.Where(r => r.Status != "Active");
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<DiscountRule> AddAsync(DiscountRule rule, CancellationToken cancellationToken = default)
    {
        await _context.DiscountRules.AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return rule;
    }

    public async Task UpdateAsync(DiscountRule rule, CancellationToken cancellationToken = default)
    {
        _context.DiscountRules.Update(rule);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DiscountRule rule, CancellationToken cancellationToken = default)
    {
        _context.DiscountRules.Remove(rule);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
using Billing.Application.Interfaces;
using Billing.Domain.Entities;

namespace Billing.Tests.Unit.Fakes;

public class FakeDiscountRuleRepository : IDiscountRuleRepository
{
    public List<DiscountRule> Rules { get; } = new();

    public Task<DiscountRule?> GetByIdAsync(int id, int tenantId, CancellationToken cancellationToken = default)
    {
        var rule = Rules.FirstOrDefault(r => r.Id == id && r.TenantId == tenantId);
        return Task.FromResult(rule);
    }

    public Task<DiscountRule?> GetByCodeAsync(string code, int tenantId, CancellationToken cancellationToken = default)
    {
        var rule = Rules.FirstOrDefault(r =>
            r.TenantId == tenantId &&
            r.Code.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(rule);
    }

    public Task<(List<DiscountRule> Items, int TotalCount)> GetPagedAsync(
        int tenantId,
        int page,
        int pageSize,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = Rules.Where(r => r.TenantId == tenantId);

        if (activeOnly.HasValue && activeOnly.Value)
        {
            query = query.Where(r => r.IsActive);
        }

        var list = query.OrderByDescending(r => r.CreatedAtUtc).ToList();
        var total = list.Count;
        var paged = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult((paged, total));
    }

    public Task<DiscountRule> AddAsync(DiscountRule rule, CancellationToken cancellationToken = default)
    {
        if (rule.Id == 0)
        {
            rule.Id = Rules.Count > 0 ? Rules.Max(r => r.Id) + 1 : 1;
        }
        Rules.Add(rule);
        return Task.FromResult(rule);
    }

    public Task UpdateAsync(DiscountRule rule, CancellationToken cancellationToken = default)
    {
        var index = Rules.FindIndex(r => r.Id == rule.Id && r.TenantId == rule.TenantId);
        if (index >= 0)
        {
            Rules[index] = rule;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(DiscountRule rule, CancellationToken cancellationToken = default)
    {
        Rules.RemoveAll(r => r.Id == rule.Id && r.TenantId == rule.TenantId);
        return Task.CompletedTask;
    }
}
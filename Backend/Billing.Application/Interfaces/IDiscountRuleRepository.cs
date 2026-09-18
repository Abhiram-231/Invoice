using Billing.Domain.Entities;

namespace Billing.Application.Interfaces;

public interface IDiscountRuleRepository
{
    Task<DiscountRule?> GetByIdAsync(int id, int tenantId, CancellationToken cancellationToken = default);
    Task<DiscountRule?> GetByCodeAsync(string code, int tenantId, CancellationToken cancellationToken = default);
    Task<(List<DiscountRule> Items, int TotalCount)> GetPagedAsync(int tenantId, int page, int pageSize, bool? activeOnly, CancellationToken cancellationToken = default);
    Task<DiscountRule> AddAsync(DiscountRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(DiscountRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(DiscountRule rule, CancellationToken cancellationToken = default);
}
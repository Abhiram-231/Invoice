using Billing.Domain.Entities;

namespace Billing.Application.Interfaces;

public interface INumberingRepository
{
    Task<NumberingSetting?> GetByDocumentTypeAsync(string documentType, int tenantId);
    Task<List<NumberingSetting>> GetAllAsync(int tenantId);
    Task<NumberingSetting> AddAsync(NumberingSetting setting);
    Task<NumberingSetting> UpdateAsync(NumberingSetting setting);
    Task<long> IncrementSequenceAsync(int settingId, int tenantId);
}

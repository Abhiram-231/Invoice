using Billing.Contracts;
using Billing.Contracts.Numbering;

namespace Billing.Application.Interfaces;

public interface INumberingSettingService
{
    Task<ApiResponse<NumberingSettingDto>> GetSettingByDocTypeAsync(string documentType, int tenantId);
    Task<ApiResponse<List<NumberingSettingDto>>> GetAllSettingsAsync(int tenantId);
    Task<ApiResponse<NumberingSettingDto>> UpdateSettingAsync(UpdateNumberingSettingRequest request, int tenantId);
    NumberPreviewResponseDto Preview(NumberPreviewRequest request);
}

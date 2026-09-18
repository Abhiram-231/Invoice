using Billing.Contracts;
using Billing.Contracts.Numbering;

namespace Billing.Application.Interfaces;

public interface INumberGenerationService
{
    Task<ApiResponse<GenerateNumberResponseDto>> GenerateNextNumberAsync(GenerateNumberRequest request, int tenantId);
    string FormatNumber(string prefix, string tokens, long sequenceNumber, int sequenceLength, string suffix, DateTime date);
    string ReplaceTokens(string text, DateTime date);
}

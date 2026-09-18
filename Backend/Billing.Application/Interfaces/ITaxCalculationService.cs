using Billing.Contracts;
using Billing.Contracts.Tax;
using Billing.Domain.Entities;

namespace Billing.Application.Interfaces;

public interface ITaxCalculationService
{
    Task<ApiResponse<TaxCalculationResultDto>> CalculateTaxesAsync(TaxCalculationRequest request, int tenantId);

    TaxCalculationResultDto Calculate(
        TaxCalculationRequest request,
        IEnumerable<TaxRate> availableRates,
        TaxSetting? tenantSettings);

    CalculatedLineItemDto CalculateLineItem(
        TaxCalculationItemRequest item,
        IEnumerable<TaxRate> applicableRates,
        bool isInclusive,
        DateTime transactionDate);

    List<AppliedTaxDto> CalculateInvoiceTaxes(
        decimal taxableSubtotal,
        IEnumerable<TaxRate> invoiceRates,
        DateTime transactionDate);
}

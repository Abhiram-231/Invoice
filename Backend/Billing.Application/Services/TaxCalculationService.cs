using Billing.Application.Interfaces;
using Billing.Contracts;
using Billing.Contracts.Tax;
using Billing.Domain.Entities;

namespace Billing.Application.Services;

public class TaxCalculationService : ITaxCalculationService
{
    private readonly ITaxRepository _taxRepository;

    public TaxCalculationService(ITaxRepository taxRepository)
    {
        _taxRepository = taxRepository;
    }

    public async Task<ApiResponse<TaxCalculationResultDto>> CalculateTaxesAsync(TaxCalculationRequest request, int tenantId)
    {
        var tenantSettings = await _taxRepository.GetSettingsAsync(tenantId);
        var availableRates = await _taxRepository.GetRatesAsync(tenantId, status: "Active");

        // Validate conflicts in active rate configuration
        var configConflictErrors = TaxValidator.ValidateConflicts(availableRates);
        if (configConflictErrors.Count > 0)
        {
            return ApiResponse<TaxCalculationResultDto>.Fail(
                "Tax configuration has conflicts.", configConflictErrors);
        }

        var result = Calculate(request, availableRates, tenantSettings);
        return ApiResponse<TaxCalculationResultDto>.Ok(result, "Taxes calculated successfully.");
    }

    public TaxCalculationResultDto Calculate(
        TaxCalculationRequest request,
        IEnumerable<TaxRate> availableRates,
        TaxSetting? tenantSettings)
    {
        var ratesList = availableRates.ToList();
        var transactionDate = request.TransactionDate ?? DateTime.UtcNow;
        var defaultPricesIncludeTax = request.PricesIncludeTax
                                     ?? tenantSettings?.PricesIncludeTax
                                     ?? string.Equals(tenantSettings?.DefaultTaxCalculation, "Inclusive", StringComparison.OrdinalIgnoreCase);

        var result = new TaxCalculationResultDto();
        decimal totalBaseAmount = 0m;
        decimal totalDiscountAmount = 0m;
        decimal totalTaxableAmount = 0m;
        decimal totalItemTaxAmount = 0m;

        // 1. Process Line Items
        foreach (var item in request.Items)
        {
            bool isItemInclusive = item.IsInclusive ?? defaultPricesIncludeTax;

            // Resolve applicable tax rates for this item
            var applicableRates = ResolveApplicableRates(
                item.TaxRateIds,
                item.TaxCodes,
                ratesList,
                request.CustomerState,
                request.TenantState ?? tenantSettings?.State,
                transactionDate,
                "Item");

            var calculatedItem = CalculateLineItem(item, applicableRates, isItemInclusive, transactionDate);
            result.LineItems.Add(calculatedItem);

            totalBaseAmount += calculatedItem.BaseAmount;
            totalDiscountAmount += calculatedItem.DiscountAmount;
            totalTaxableAmount += calculatedItem.TaxableAmount;
            totalItemTaxAmount += calculatedItem.TotalTaxAmount;
        }

        // 2. Process Invoice-level Taxes
        var invoiceRates = ResolveApplicableRates(
            request.InvoiceLevelTaxRateIds,
            request.InvoiceLevelTaxCodes,
            ratesList,
            request.CustomerState,
            request.TenantState ?? tenantSettings?.State,
            transactionDate,
            "Invoice");

        var invoiceTaxes = CalculateInvoiceTaxes(totalTaxableAmount, invoiceRates, transactionDate);
        decimal totalInvoiceTaxAmount = invoiceTaxes.Sum(t => t.TaxAmount);

        decimal grandTotal = totalTaxableAmount + totalItemTaxAmount + totalInvoiceTaxAmount;
        if (defaultPricesIncludeTax)
        {
            // If line items were inclusive, gross amount already included item tax
            grandTotal = result.LineItems.Sum(li => li.GrossAmount) + totalInvoiceTaxAmount;
        }

        // 3. Build Summary & Tax Breakdown
        var summary = new TaxCalculationSummaryDto
        {
            TotalBaseAmount = Math.Round(totalBaseAmount, 2, MidpointRounding.AwayFromZero),
            TotalDiscountAmount = Math.Round(totalDiscountAmount, 2, MidpointRounding.AwayFromZero),
            TotalTaxableAmount = Math.Round(totalTaxableAmount, 2, MidpointRounding.AwayFromZero),
            TotalItemTaxAmount = Math.Round(totalItemTaxAmount, 2, MidpointRounding.AwayFromZero),
            TotalInvoiceTaxAmount = Math.Round(totalInvoiceTaxAmount, 2, MidpointRounding.AwayFromZero),
            TotalTaxAmount = Math.Round(totalItemTaxAmount + totalInvoiceTaxAmount, 2, MidpointRounding.AwayFromZero),
            GrandTotal = Math.Round(grandTotal, 2, MidpointRounding.AwayFromZero),
            InvoiceTaxes = invoiceTaxes
        };

        // Combine item-level and invoice-level applied taxes for breakdown
        var allApplied = result.LineItems.SelectMany(li => li.AppliedTaxes).Concat(invoiceTaxes);
        summary.TaxBreakdown = allApplied
            .GroupBy(t => new { t.TaxType, t.Code, t.Name, t.Rate })
            .Select(g => new TaxSummaryComponentDto
            {
                TaxType = g.Key.TaxType,
                Code = g.Key.Code,
                Name = g.Key.Name,
                Rate = g.Key.Rate,
                TotalTaxAmount = Math.Round(g.Sum(x => x.TaxAmount), 2, MidpointRounding.AwayFromZero)
            })
            .OrderBy(b => b.TaxType)
            .ThenBy(b => b.Rate)
            .ToList();

        result.Summary = summary;
        return result;
    }

    public CalculatedLineItemDto CalculateLineItem(
        TaxCalculationItemRequest item,
        IEnumerable<TaxRate> applicableRates,
        bool isInclusive,
        DateTime transactionDate)
    {
        decimal quantity = item.Quantity > 0 ? item.Quantity : 1m;
        decimal baseAmount = Math.Round(quantity * item.UnitPrice, 2, MidpointRounding.AwayFromZero);

        decimal discountAmount = 0m;
        if (item.DiscountAmount > 0)
        {
            discountAmount = item.DiscountAmount;
        }
        else if (item.DiscountPercent > 0)
        {
            discountAmount = Math.Round(baseAmount * (item.DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        }

        decimal netLineAmount = Math.Max(0m, baseAmount - discountAmount);

        // Filter and sort applicable rates by active date and priority
        var activeRates = applicableRates
            .Where(r => TaxValidator.IsTaxActiveOnDate(r, transactionDate))
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Id)
            .ToList();

        var calculated = new CalculatedLineItemDto
        {
            ItemId = item.ItemId,
            Name = item.Name,
            Quantity = quantity,
            UnitPrice = item.UnitPrice,
            DiscountPercent = item.DiscountPercent,
            BaseAmount = baseAmount,
            DiscountAmount = discountAmount
        };

        if (activeRates.Count == 0)
        {
            calculated.TaxableAmount = netLineAmount;
            calculated.TotalTaxAmount = 0m;
            calculated.GrossAmount = netLineAmount;
            return calculated;
        }

        if (isInclusive)
        {
            // Inclusive Tax Calculation
            // 1. Non-compound taxes are reverse calculated together
            var nonCompoundRates = activeRates.Where(r => !r.IsCompound).ToList();
            var compoundRates = activeRates.Where(r => r.IsCompound).ToList();

            decimal totalNonCompoundPercent = nonCompoundRates.Sum(r => r.Rate);
            decimal taxableBase = totalNonCompoundPercent > 0
                ? Math.Round(netLineAmount / (1m + (totalNonCompoundPercent / 100m)), 4, MidpointRounding.AwayFromZero)
                : netLineAmount;

            var appliedList = new List<AppliedTaxDto>();
            decimal runningTotalTax = 0m;

            foreach (var rate in nonCompoundRates)
            {
                decimal taxAmt = Math.Round(taxableBase * (rate.Rate / 100m), 2, MidpointRounding.AwayFromZero);
                runningTotalTax += taxAmt;
                appliedList.Add(new AppliedTaxDto
                {
                    TaxRateId = rate.Id > 0 ? rate.Id : null,
                    Name = rate.Name,
                    Code = rate.Code,
                    TaxType = rate.TaxType,
                    Rate = rate.Rate,
                    TaxAmount = taxAmt,
                    IsCompound = false,
                    Priority = rate.Priority,
                    ApplicationLevel = rate.ApplicationLevel
                });
            }

            // Compound taxes applied sequentially
            decimal currentTaxable = taxableBase + runningTotalTax;
            foreach (var rate in compoundRates)
            {
                decimal taxAmt = Math.Round(currentTaxable * (rate.Rate / 100m), 2, MidpointRounding.AwayFromZero);
                runningTotalTax += taxAmt;
                currentTaxable += taxAmt;

                appliedList.Add(new AppliedTaxDto
                {
                    TaxRateId = rate.Id > 0 ? rate.Id : null,
                    Name = rate.Name,
                    Code = rate.Code,
                    TaxType = rate.TaxType,
                    Rate = rate.Rate,
                    TaxAmount = taxAmt,
                    IsCompound = true,
                    Priority = rate.Priority,
                    ApplicationLevel = rate.ApplicationLevel
                });
            }

            calculated.AppliedTaxes = appliedList;
            calculated.TotalTaxAmount = Math.Round(runningTotalTax, 2, MidpointRounding.AwayFromZero);
            calculated.TaxableAmount = Math.Round(netLineAmount - calculated.TotalTaxAmount, 2, MidpointRounding.AwayFromZero);
            calculated.GrossAmount = netLineAmount; // Item price already included tax
        }
        else
        {
            // Exclusive Tax Calculation
            calculated.TaxableAmount = netLineAmount;

            var appliedList = new List<AppliedTaxDto>();
            decimal accumulatedForCompound = netLineAmount;
            decimal totalTax = 0m;

            foreach (var rate in activeRates)
            {
                decimal baseForThisTax = rate.IsCompound ? accumulatedForCompound : netLineAmount;
                decimal taxAmt = Math.Round(baseForThisTax * (rate.Rate / 100m), 2, MidpointRounding.AwayFromZero);

                accumulatedForCompound += taxAmt;
                totalTax += taxAmt;

                appliedList.Add(new AppliedTaxDto
                {
                    TaxRateId = rate.Id > 0 ? rate.Id : null,
                    Name = rate.Name,
                    Code = rate.Code,
                    TaxType = rate.TaxType,
                    Rate = rate.Rate,
                    TaxAmount = taxAmt,
                    IsCompound = rate.IsCompound,
                    Priority = rate.Priority,
                    ApplicationLevel = rate.ApplicationLevel
                });
            }

            calculated.AppliedTaxes = appliedList;
            calculated.TotalTaxAmount = Math.Round(totalTax, 2, MidpointRounding.AwayFromZero);
            calculated.GrossAmount = Math.Round(netLineAmount + calculated.TotalTaxAmount, 2, MidpointRounding.AwayFromZero);
        }

        return calculated;
    }

    public List<AppliedTaxDto> CalculateInvoiceTaxes(
        decimal taxableSubtotal,
        IEnumerable<TaxRate> invoiceRates,
        DateTime transactionDate)
    {
        var activeRates = invoiceRates
            .Where(r => TaxValidator.IsTaxActiveOnDate(r, transactionDate))
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Id)
            .ToList();

        var appliedList = new List<AppliedTaxDto>();
        decimal accumulated = taxableSubtotal;

        foreach (var rate in activeRates)
        {
            decimal baseAmount = rate.IsCompound ? accumulated : taxableSubtotal;
            decimal taxAmt = Math.Round(baseAmount * (rate.Rate / 100m), 2, MidpointRounding.AwayFromZero);

            accumulated += taxAmt;
            appliedList.Add(new AppliedTaxDto
            {
                TaxRateId = rate.Id > 0 ? rate.Id : null,
                Name = rate.Name,
                Code = rate.Code,
                TaxType = rate.TaxType,
                Rate = rate.Rate,
                TaxAmount = taxAmt,
                IsCompound = rate.IsCompound,
                Priority = rate.Priority,
                ApplicationLevel = "Invoice"
            });
        }

        return appliedList;
    }

    private List<TaxRate> ResolveApplicableRates(
        List<int>? taxRateIds,
        List<string>? taxCodes,
        List<TaxRate> availableRates,
        string? customerState,
        string? tenantState,
        DateTime transactionDate,
        string applicationLevel)
    {
        var matched = new List<TaxRate>();

        if (taxRateIds != null && taxRateIds.Count > 0)
        {
            matched.AddRange(availableRates.Where(r => taxRateIds.Contains(r.Id)));
        }

        if (taxCodes != null && taxCodes.Count > 0)
        {
            var codeUpper = taxCodes.Select(c => c.Trim().ToUpperInvariant()).ToHashSet();
            matched.AddRange(availableRates.Where(r => codeUpper.Contains(r.Code.ToUpper())));
        }

        var distinctRates = matched.DistinctBy(r => r.Id).ToList();

        // IBMSBE-004: GST Intra-state vs Inter-state selection & splitting logic
        var result = new List<TaxRate>();

        foreach (var rate in distinctRates)
        {
            if (string.Equals(rate.TaxType, "GST", StringComparison.OrdinalIgnoreCase))
            {
                // Determine whether intra-state or inter-state
                bool isIntraState = !string.IsNullOrWhiteSpace(customerState)
                    && !string.IsNullOrWhiteSpace(tenantState)
                    && string.Equals(customerState.Trim(), tenantState.Trim(), StringComparison.OrdinalIgnoreCase);

                if (isIntraState)
                {
                    // Split into CGST and SGST (each half rate)
                    decimal halfRate = Math.Round(rate.Rate / 2m, 2, MidpointRounding.AwayFromZero);

                    result.Add(new TaxRate
                    {
                        Id = 0,
                        TenantId = rate.TenantId,
                        Name = $"CGST ({halfRate}%)",
                        Code = $"{rate.Code}_CGST",
                        TaxType = "CGST",
                        Rate = halfRate,
                        IsCompound = rate.IsCompound,
                        IsInclusive = rate.IsInclusive,
                        ApplicationLevel = rate.ApplicationLevel,
                        Priority = rate.Priority,
                        EffectiveFrom = rate.EffectiveFrom,
                        EffectiveTo = rate.EffectiveTo,
                        Status = "Active"
                    });

                    result.Add(new TaxRate
                    {
                        Id = 0,
                        TenantId = rate.TenantId,
                        Name = $"SGST ({halfRate}%)",
                        Code = $"{rate.Code}_SGST",
                        TaxType = "SGST",
                        Rate = halfRate,
                        IsCompound = rate.IsCompound,
                        IsInclusive = rate.IsInclusive,
                        ApplicationLevel = rate.ApplicationLevel,
                        Priority = rate.Priority,
                        EffectiveFrom = rate.EffectiveFrom,
                        EffectiveTo = rate.EffectiveTo,
                        Status = "Active"
                    });
                }
                else if (!string.IsNullOrWhiteSpace(customerState) && !string.IsNullOrWhiteSpace(tenantState))
                {
                    // Inter-state: treat as IGST
                    result.Add(new TaxRate
                    {
                        Id = 0,
                        TenantId = rate.TenantId,
                        Name = $"IGST ({rate.Rate}%)",
                        Code = $"{rate.Code}_IGST",
                        TaxType = "IGST",
                        Rate = rate.Rate,
                        IsCompound = rate.IsCompound,
                        IsInclusive = rate.IsInclusive,
                        ApplicationLevel = rate.ApplicationLevel,
                        Priority = rate.Priority,
                        EffectiveFrom = rate.EffectiveFrom,
                        EffectiveTo = rate.EffectiveTo,
                        Status = "Active"
                    });
                }
                else
                {
                    // Fallback to original rate if states are unspecified
                    result.Add(rate);
                }
            }
            else
            {
                result.Add(rate);
            }
        }

        return result;
    }
}

using Billing.Application.Interfaces;
using Billing.Contracts;
using Billing.Contracts.Financial;

namespace Billing.Application.Services;

public class FinancialCalculationEngine : IFinancialCalculationEngine
{
    private readonly IDiscountService _discountService;
    private readonly ITaxCalculationService _taxCalculationService;
    private readonly IChargeCalculationService _chargeCalculationService;
    private readonly ITaxRepository _taxRepository;

    public FinancialCalculationEngine(
        IDiscountService discountService,
        ITaxCalculationService taxCalculationService,
        IChargeCalculationService chargeCalculationService,
        ITaxRepository taxRepository)
    {
        _discountService = discountService;
        _taxCalculationService = taxCalculationService;
        _chargeCalculationService = chargeCalculationService;
        _taxRepository = taxRepository;
    }

    public async Task<ApiResponse<FinancialCalculationResultDto>> CalculateAsync(
        FinancialCalculationRequest request,
        int tenantId,
        string? userRole = null,
        string? userName = null)
    {
        if (tenantId <= 0)
        {
            return ApiResponse<FinancialCalculationResultDto>.Fail("Invalid tenant identifier", "Tenant ID must be greater than 0.");
        }

        if (request == null || request.Items == null || !request.Items.Any())
        {
            return ApiResponse<FinancialCalculationResultDto>.Fail("Validation failed", "At least one line item is required for financial calculation.");
        }

        var result = new FinancialCalculationResultDto
        {
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "INR" : request.Currency.Trim().ToUpperInvariant()
        };

        decimal totalGross = 0m;
        decimal totalLineDiscount = 0m;
        decimal totalNet = 0m;
        decimal totalLineTax = 0m;
        decimal totalLineExclusiveTax = 0m;

        // 1. Process Line Items (Deterministic order: Gross -> Line Discount -> Line Taxes)
        foreach (var item in request.Items)
        {
            var gross = Math.Round(item.UnitPrice * item.Quantity, 2, MidpointRounding.AwayFromZero);

            // Calculate Line Discount
            decimal lineDiscountAmount = 0m;
            if (item.LineDiscountValue.HasValue && item.LineDiscountValue.Value > 0)
            {
                var isFixed = string.Equals(item.LineDiscountType, "Fixed", StringComparison.OrdinalIgnoreCase);
                if (isFixed)
                {
                    lineDiscountAmount = Math.Min(gross, item.LineDiscountValue.Value);
                }
                else
                {
                    var percent = Math.Min(100.00m, item.LineDiscountValue.Value);
                    lineDiscountAmount = Math.Round(gross * (percent / 100.00m), 2, MidpointRounding.AwayFromZero);
                }
            }

            var net = Math.Max(0m, gross - lineDiscountAmount);

            // Calculate Line Tax
            decimal taxRate = item.TaxRatePercent.GetValueOrDefault(0m);
            if (item.TaxRateId.HasValue && taxRate == 0m)
            {
                var rateEntity = await _taxRepository.GetRateByIdAsync(item.TaxRateId.Value, tenantId);
                if (rateEntity != null && rateEntity.IsActive)
                {
                    taxRate = rateEntity.Rate;
                }
            }

            bool isInclusive = item.IsTaxInclusive.GetValueOrDefault(request.PricesIncludeTax.GetValueOrDefault(false));
            decimal taxAmount = 0m;
            decimal lineTotal = 0m;

            if (taxRate > 0)
            {
                if (isInclusive)
                {
                    // Inclusive formula: Tax = Net - (Net / (1 + Rate / 100))
                    var baseAmount = net / (1.00m + (taxRate / 100.00m));
                    taxAmount = Math.Round(net - baseAmount, 2, MidpointRounding.AwayFromZero);
                    lineTotal = net; // Tax is already included
                }
                else
                {
                    // Exclusive formula: Tax = Net * (Rate / 100)
                    taxAmount = Math.Round(net * (taxRate / 100.00m), 2, MidpointRounding.AwayFromZero);
                    lineTotal = net + taxAmount;
                    totalLineExclusiveTax += taxAmount;
                }
            }
            else
            {
                lineTotal = net;
            }

            totalGross += gross;
            totalLineDiscount += lineDiscountAmount;
            totalNet += net;
            totalLineTax += taxAmount;

            result.Items.Add(new CalculatedFinancialLineDto
            {
                Name = item.Name,
                ProductCode = item.ProductCode,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                GrossAmount = gross,
                DiscountAmount = lineDiscountAmount,
                NetAmount = net,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                IsInclusive = isInclusive,
                LineTotal = lineTotal
            });

            if (lineDiscountAmount > 0)
            {
                result.AppliedDiscounts.Add(new FinancialAppliedDiscountDto
                {
                    Scope = "Line",
                    Type = item.LineDiscountType ?? "Percentage",
                    Value = item.LineDiscountValue.GetValueOrDefault(),
                    Amount = lineDiscountAmount
                });
            }

            if (taxAmount > 0)
            {
                result.AppliedTaxes.Add(new FinancialAppliedTaxDto
                {
                    TaxName = !string.IsNullOrWhiteSpace(item.TaxCode) ? item.TaxCode : "GST",
                    TaxCode = item.TaxCode,
                    Rate = taxRate,
                    TaxableAmount = net,
                    TaxAmount = taxAmount,
                    Level = "Item"
                });
            }
        }

        result.GrossSubtotal = Math.Round(totalGross, 2, MidpointRounding.AwayFromZero);
        result.TotalLineDiscounts = Math.Round(totalLineDiscount, 2, MidpointRounding.AwayFromZero);
        result.NetItemSubtotal = Math.Round(totalNet, 2, MidpointRounding.AwayFromZero);
        result.LineTaxesTotal = Math.Round(totalLineTax, 2, MidpointRounding.AwayFromZero);

        // 2. Invoice-level Discount
        decimal invoiceDiscountAmount = 0m;
        if (request.InvoiceDiscount != null && request.InvoiceDiscount.Value > 0)
        {
            var isFixed = string.Equals(request.InvoiceDiscount.DiscountType, "Fixed", StringComparison.OrdinalIgnoreCase);
            if (isFixed)
            {
                invoiceDiscountAmount = Math.Min(result.NetItemSubtotal, request.InvoiceDiscount.Value);
            }
            else
            {
                var percent = Math.Min(100.00m, request.InvoiceDiscount.Value);
                invoiceDiscountAmount = Math.Round(result.NetItemSubtotal * (percent / 100.00m), 2, MidpointRounding.AwayFromZero);
            }

            result.AppliedDiscounts.Add(new FinancialAppliedDiscountDto
            {
                Scope = "Invoice",
                Type = request.InvoiceDiscount.DiscountType ?? "Percentage",
                Value = request.InvoiceDiscount.Value,
                Amount = invoiceDiscountAmount,
                Reason = request.InvoiceDiscount.OverrideReason
            });
        }

        result.InvoiceDiscountAmount = Math.Round(invoiceDiscountAmount, 2, MidpointRounding.AwayFromZero);
        result.TaxableSubtotal = Math.Max(0m, result.NetItemSubtotal - result.InvoiceDiscountAmount);

        // 3. Charges
        decimal totalCharges = 0m;
        decimal totalTaxOnCharges = 0m;

        if (request.Charges != null && request.Charges.Any())
        {
            foreach (var ch in request.Charges)
            {
                decimal chargeAmount = 0m;
                var isFixed = string.Equals(ch.CalculationType, "Fixed", StringComparison.OrdinalIgnoreCase);
                if (isFixed)
                {
                    chargeAmount = ch.Amount;
                }
                else
                {
                    var percent = Math.Min(100.00m, ch.Amount);
                    chargeAmount = Math.Round(result.NetItemSubtotal * (percent / 100.00m), 2, MidpointRounding.AwayFromZero);
                }

                decimal taxOnCharge = 0m;
                if (ch.IsTaxable && ch.TaxRatePercent.HasValue && ch.TaxRatePercent.Value > 0)
                {
                    taxOnCharge = Math.Round(chargeAmount * (ch.TaxRatePercent.Value / 100.00m), 2, MidpointRounding.AwayFromZero);
                    totalTaxOnCharges += taxOnCharge;

                    result.AppliedTaxes.Add(new FinancialAppliedTaxDto
                    {
                        TaxName = $"Tax on {ch.Name}",
                        Rate = ch.TaxRatePercent.Value,
                        TaxableAmount = chargeAmount,
                        TaxAmount = taxOnCharge,
                        Level = "Charge"
                    });
                }

                totalCharges += chargeAmount;

                result.AppliedCharges.Add(new FinancialAppliedChargeDto
                {
                    Name = ch.Name,
                    ChargeCode = ch.ChargeCode,
                    ChargeType = ch.ChargeType,
                    ChargeAmount = chargeAmount,
                    IsTaxable = ch.IsTaxable,
                    TaxAmount = taxOnCharge,
                    TotalAmount = chargeAmount + taxOnCharge
                });
            }
        }

        result.ChargesTotal = Math.Round(totalCharges, 2, MidpointRounding.AwayFromZero);
        result.TaxOnChargesTotal = Math.Round(totalTaxOnCharges, 2, MidpointRounding.AwayFromZero);
        result.InvoiceTaxesTotal = 0m; // Currently line-level taxes are standard; extension point ready
        result.TotalTaxes = Math.Round(result.LineTaxesTotal + result.InvoiceTaxesTotal + result.TaxOnChargesTotal, 2, MidpointRounding.AwayFromZero);

        // 4. Final Grand Total:
        // Base Net - Invoice Discount + Exclusive Taxes + Charges + Tax on Charges
        var grandTotal = (result.NetItemSubtotal - result.InvoiceDiscountAmount) + totalLineExclusiveTax + result.ChargesTotal + result.TaxOnChargesTotal;
        result.GrandTotal = Math.Round(Math.Max(0m, grandTotal), 2, MidpointRounding.AwayFromZero);

        return ApiResponse<FinancialCalculationResultDto>.Ok(result, "Financial calculation completed successfully.");
    }
}

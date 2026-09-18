using Billing.Application.Services;
using Billing.Contracts.Financial;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Billing.Tests.Unit.Fakes;
using Xunit;

namespace Billing.Tests.Unit;

public class FinancialEngineTests
{
    private readonly FakeDiscountRuleRepository _discountRepo;
    private readonly DiscountService _discountService;
    private readonly FakeTaxRepository _taxRepo;
    private readonly TaxCalculationService _taxService;
    private readonly FakeChargeRepository _chargeRepo;
    private readonly ChargeCalculationService _chargeService;
    private readonly FinancialCalculationEngine _engine;

    public FinancialEngineTests()
    {
        _discountRepo = new FakeDiscountRuleRepository();
        _discountService = new DiscountService(
            _discountRepo,
            new FakeAuditLogRepository(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DiscountService>.Instance);

        _taxRepo = new FakeTaxRepository();
        _taxService = new TaxCalculationService(_taxRepo);

        _chargeRepo = new FakeChargeRepository();
        _chargeService = new ChargeCalculationService(_chargeRepo);

        _engine = new FinancialCalculationEngine(
            _discountService,
            _taxService,
            _chargeService,
            _taxRepo);
    }

    [Fact]
    public async Task Calculate_DeterministicTaxDiscountChargesOrder_ComputesExactGrandTotal()
    {
        // Setup line items:
        // Item 1: Qty 2 @ 500 = Gross 1000. Line Discount 10% = 100 => Net 900. Tax 18% GST (Exclusive) = 162. Total = 1062.
        // Item 2: Qty 1 @ 1000 = Gross 1000. Line Discount 0 => Net 1000. Tax 5% VAT (Exclusive) = 50. Total = 1050.
        // Net Subtotal = 1900. Total Line Taxes = 212.
        // Invoice Discount = 100 (Fixed). Taxable after invoice discount = 1800.
        // Charges:
        // Charge 1: Shipping = 50 (Taxable @ 18% = 9).
        // Charge 2: Handling = 30 (Non-taxable).
        // Total Charges = 80. Tax on charges = 9.
        // Grand Total = (Net 1900 - InvDiscount 100) + LineTaxes 212 + Charges 80 + TaxOnCharges 9 = 2101.

        var request = new FinancialCalculationRequest
        {
            Items = new List<FinancialLineItemRequest>
            {
                new()
                {
                    Name = "Widget A",
                    UnitPrice = 500.00m,
                    Quantity = 2,
                    LineDiscountType = "Percentage",
                    LineDiscountValue = 10.00m,
                    TaxRatePercent = 18.00m,
                    IsTaxInclusive = false
                },
                new()
                {
                    Name = "Widget B",
                    UnitPrice = 1000.00m,
                    Quantity = 1,
                    TaxRatePercent = 5.00m,
                    IsTaxInclusive = false
                }
            },
            InvoiceDiscount = new FinancialInvoiceDiscountRequest
            {
                DiscountType = "Fixed",
                Value = 100.00m
            },
            Charges = new List<FinancialChargeRequest>
            {
                new()
                {
                    Name = "Shipping",
                    Amount = 50.00m,
                    CalculationType = "Fixed",
                    IsTaxable = true,
                    TaxRatePercent = 18.00m
                },
                new()
                {
                    Name = "Handling",
                    Amount = 30.00m,
                    CalculationType = "Fixed",
                    IsTaxable = false
                }
            }
        };

        var result = await _engine.CalculateAsync(request, tenantId: 1);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        var data = result.Data;
        Assert.Equal(2000.00m, data.GrossSubtotal);
        Assert.Equal(100.00m, data.TotalLineDiscounts);
        Assert.Equal(1900.00m, data.NetItemSubtotal);
        Assert.Equal(100.00m, data.InvoiceDiscountAmount);
        Assert.Equal(1800.00m, data.TaxableSubtotal);
        Assert.Equal(212.00m, data.LineTaxesTotal);
        Assert.Equal(80.00m, data.ChargesTotal);
        Assert.Equal(9.00m, data.TaxOnChargesTotal);
        Assert.Equal(221.00m, data.TotalTaxes); // 212 + 9
        Assert.Equal(2101.00m, data.GrandTotal);
    }

    [Fact]
    public async Task Calculate_TaxInclusivePricing_ExtractsTaxAccurately()
    {
        // 1 item @ 1180 INR Tax Inclusive with 18% GST
        // Gross = 1180. Tax inside = 1180 - (1180 / 1.18) = 180.
        // Base Net = 1180. (Tax is inside).
        // Grand Total = 1180.
        var request = new FinancialCalculationRequest
        {
            Items = new List<FinancialLineItemRequest>
            {
                new()
                {
                    Name = "Tax Inclusive Phone",
                    UnitPrice = 1180.00m,
                    Quantity = 1,
                    TaxRatePercent = 18.00m,
                    IsTaxInclusive = true
                }
            }
        };

        var result = await _engine.CalculateAsync(request, tenantId: 1);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        var data = result.Data;
        Assert.Equal(1180.00m, data.GrossSubtotal);
        Assert.Equal(180.00m, data.LineTaxesTotal);
        Assert.Equal(1180.00m, data.GrandTotal);
    }

    [Fact]
    public async Task Calculate_EmptyLineItems_ReturnsValidationError()
    {
        var request = new FinancialCalculationRequest
        {
            Items = new List<FinancialLineItemRequest>()
        };

        var result = await _engine.CalculateAsync(request, tenantId: 1);
        Assert.False(result.Success);
        Assert.Contains("At least one line item is required", result.Errors.First());
    }
}

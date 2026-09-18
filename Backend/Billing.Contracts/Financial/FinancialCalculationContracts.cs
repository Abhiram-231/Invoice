using System.ComponentModel.DataAnnotations;

namespace Billing.Contracts.Financial;

public class FinancialCalculationRequest
{
    [Required]
    public List<FinancialLineItemRequest> Items { get; set; } = new();

    public FinancialInvoiceDiscountRequest? InvoiceDiscount { get; set; }

    public List<FinancialChargeRequest>? Charges { get; set; }

    public DateTime? TransactionDate { get; set; }

    public bool? PricesIncludeTax { get; set; }

    public string Currency { get; set; } = "INR";
}

public class FinancialLineItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ProductCode { get; set; }

    [Range(0.01, 999999999.99)]
    public decimal UnitPrice { get; set; }

    [Range(0.01, 999999)]
    public decimal Quantity { get; set; } = 1;

    public string? LineDiscountType { get; set; } // "Percentage" or "Fixed"
    public decimal? LineDiscountValue { get; set; }

    public int? TaxRateId { get; set; }
    public string? TaxCode { get; set; }
    public decimal? TaxRatePercent { get; set; }
    public bool? IsTaxInclusive { get; set; }
}

public class FinancialInvoiceDiscountRequest
{
    public string? DiscountType { get; set; } // "Percentage" or "Fixed"
    public decimal Value { get; set; }
    public string? RuleCode { get; set; }
    public string? OverrideReason { get; set; }
}

public class FinancialChargeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ChargeCode { get; set; }
    public string ChargeType { get; set; } = "Custom"; // Shipping, Handling, ConvenienceFee, LateFee, Custom
    public string CalculationType { get; set; } = "Fixed"; // Fixed, Percentage
    public decimal Amount { get; set; }
    public bool IsTaxable { get; set; } = true;
    public decimal? TaxRatePercent { get; set; }
}

public class FinancialCalculationResultDto
{
    public decimal GrossSubtotal { get; set; }
    public decimal TotalLineDiscounts { get; set; }
    public decimal NetItemSubtotal { get; set; }
    public decimal InvoiceDiscountAmount { get; set; }
    public decimal TaxableSubtotal { get; set; }
    public decimal LineTaxesTotal { get; set; }
    public decimal InvoiceTaxesTotal { get; set; }
    public decimal TotalTaxes { get; set; }
    public decimal ChargesTotal { get; set; }
    public decimal TaxOnChargesTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string Currency { get; set; } = "INR";

    public List<CalculatedFinancialLineDto> Items { get; set; } = new();
    public List<FinancialAppliedDiscountDto> AppliedDiscounts { get; set; } = new();
    public List<FinancialAppliedTaxDto> AppliedTaxes { get; set; } = new();
    public List<FinancialAppliedChargeDto> AppliedCharges { get; set; } = new();
}

public class CalculatedFinancialLineDto
{
    public string Name { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public bool IsInclusive { get; set; }
    public decimal LineTotal { get; set; }
}

public class FinancialAppliedDiscountDto
{
    public string Scope { get; set; } = "Invoice"; // "Line" or "Invoice"
    public string Type { get; set; } = "Percentage";
    public decimal Value { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

public class FinancialAppliedTaxDto
{
    public string TaxName { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public decimal Rate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Level { get; set; } = "Item"; // "Item", "Invoice", "Charge"
}

public class FinancialAppliedChargeDto
{
    public string Name { get; set; } = string.Empty;
    public string? ChargeCode { get; set; }
    public string ChargeType { get; set; } = "Custom";
    public decimal ChargeAmount { get; set; }
    public bool IsTaxable { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

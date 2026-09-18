namespace Billing.Contracts.Tax;

public class TaxCalculationItemRequest
{
    public int? ItemId { get; set; }
    public string? Name { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public bool? IsInclusive { get; set; }
    public List<int> TaxRateIds { get; set; } = new();
    public List<string> TaxCodes { get; set; } = new();
    public string? HsnSacCode { get; set; }
}

public class TaxCalculationRequest
{
    public DateTime? TransactionDate { get; set; }
    public string? CustomerState { get; set; }
    public string? TenantState { get; set; }
    public bool? PricesIncludeTax { get; set; }
    public List<TaxCalculationItemRequest> Items { get; set; } = new();
    public List<int> InvoiceLevelTaxRateIds { get; set; } = new();
    public List<string> InvoiceLevelTaxCodes { get; set; } = new();
}

public class AppliedTaxDto
{
    public int? TaxRateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TaxType { get; set; } = "GST";
    public decimal Rate { get; set; }
    public decimal TaxAmount { get; set; }
    public bool IsCompound { get; set; }
    public int Priority { get; set; } = 1;
    public string ApplicationLevel { get; set; } = "Item";
}

public class CalculatedLineItemDto
{
    public int? ItemId { get; set; }
    public string? Name { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal GrossAmount { get; set; }
    public List<AppliedTaxDto> AppliedTaxes { get; set; } = new();
}

public class TaxSummaryComponentDto
{
    public string TaxType { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal TotalTaxAmount { get; set; }
}

public class TaxCalculationSummaryDto
{
    public decimal TotalBaseAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalTaxableAmount { get; set; }
    public decimal TotalItemTaxAmount { get; set; }
    public decimal TotalInvoiceTaxAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public List<AppliedTaxDto> InvoiceTaxes { get; set; } = new();
    public List<TaxSummaryComponentDto> TaxBreakdown { get; set; } = new();
}

public class TaxCalculationResultDto
{
    public List<CalculatedLineItemDto> LineItems { get; set; } = new();
    public TaxCalculationSummaryDto Summary { get; set; } = new();
}

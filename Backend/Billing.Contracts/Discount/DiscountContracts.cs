namespace Billing.Contracts.Discount;

public class DiscountRuleDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "Percentage";
    public string Scope { get; set; } = "Invoice";
    public decimal Value { get; set; }
    public decimal? MinInvoiceAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
    public string? ApplicableRole { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class CreateDiscountRuleRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "Percentage";
    public string Scope { get; set; } = "Invoice";
    public decimal Value { get; set; }
    public decimal? MinInvoiceAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public string? ApplicableRole { get; set; }
}

public class UpdateDiscountRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Value { get; set; }
    public decimal? MinInvoiceAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public string Status { get; set; } = "Active";
    public string? ApplicableRole { get; set; }
}

public class CalculateLineDiscountRequest
{
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public string DiscountType { get; set; } = "Percentage";
    public decimal DiscountValue { get; set; }
    public string? DiscountCode { get; set; }
}

public class LineDiscountResultDto
{
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal OriginalLineTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountedLineTotal { get; set; }
    public string AppliedDiscountType { get; set; } = "Percentage";
    public decimal AppliedDiscountValue { get; set; }
    public string? AppliedDiscountCode { get; set; }
}

public class CalculateInvoiceDiscountRequest
{
    public List<CalculateLineDiscountRequest> LineItems { get; set; } = new();
    public decimal? InvoiceSubtotal { get; set; }
    public string? InvoiceDiscountType { get; set; }
    public decimal? InvoiceDiscountValue { get; set; }
    public string? DiscountCode { get; set; }
    public string? UserRole { get; set; }
    public string? UserName { get; set; }
    public bool IsManualOverride { get; set; }
    public string? OverrideReason { get; set; }
    public string? InvoiceId { get; set; }
}

public class InvoiceDiscountResultDto
{
    public decimal GrossSubtotal { get; set; }
    public decimal TotalLineDiscounts { get; set; }
    public decimal NetSubtotal { get; set; }
    public decimal InvoiceDiscountAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal FinalTotal { get; set; }
    public List<LineDiscountResultDto> LineItemResults { get; set; } = new();
    public bool IsOverrideApplied { get; set; }
    public string? OverrideReason { get; set; }
    public bool IsValid { get; set; } = true;
    public List<string> ValidationErrors { get; set; } = new();
}
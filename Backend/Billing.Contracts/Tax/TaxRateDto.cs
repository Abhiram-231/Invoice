namespace Billing.Contracts.Tax;

public class TaxRateDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TaxType { get; set; } = "GST";
    public decimal Rate { get; set; }
    public string? Description { get; set; }
    public bool IsCompound { get; set; }
    public bool IsInclusive { get; set; }
    public string ApplicationLevel { get; set; } = "Item";
    public int Priority { get; set; } = 1;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

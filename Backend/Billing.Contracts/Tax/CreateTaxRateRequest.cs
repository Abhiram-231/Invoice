namespace Billing.Contracts.Tax;

public class CreateTaxRateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TaxType { get; set; } = "GST";
    public decimal Rate { get; set; }
    public string? Description { get; set; }
    public bool IsCompound { get; set; } = false;
    public bool IsInclusive { get; set; } = false;
    public string ApplicationLevel { get; set; } = "Item";
    public int Priority { get; set; } = 1;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string Status { get; set; } = "Active";
}

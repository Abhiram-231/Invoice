namespace Billing.Contracts.Tax;

public class UpdateTaxRateRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? TaxType { get; set; }
    public decimal? Rate { get; set; }
    public string? Description { get; set; }
    public bool? IsCompound { get; set; }
    public bool? IsInclusive { get; set; }
    public string? ApplicationLevel { get; set; }
    public int? Priority { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Status { get; set; }
}

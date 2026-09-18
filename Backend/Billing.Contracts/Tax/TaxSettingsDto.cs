namespace Billing.Contracts.Tax;

public class TaxSettingsDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public bool IsTaxEnabled { get; set; } = true;
    public string DefaultTaxCalculation { get; set; } = "Exclusive";
    public bool PricesIncludeTax { get; set; } = false;
    public int? DefaultTaxRateId { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public string TaxNumberLabel { get; set; } = "GSTIN";
    public bool EnableMultipleTaxes { get; set; } = true;
    public string? State { get; set; }
    public List<TaxRateDto> TaxRates { get; set; } = new();
}

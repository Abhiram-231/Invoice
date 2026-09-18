namespace Billing.Contracts.Tax;

public class UpdateTaxSettingsRequest
{
    public bool? IsTaxEnabled { get; set; }
    public string? DefaultTaxCalculation { get; set; }
    public bool? PricesIncludeTax { get; set; }
    public int? DefaultTaxRateId { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public string? TaxNumberLabel { get; set; }
    public bool? EnableMultipleTaxes { get; set; }
    public string? State { get; set; }
    public List<CreateTaxRateRequest>? TaxRates { get; set; }
}

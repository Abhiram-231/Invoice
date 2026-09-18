using Billing.Domain.Enums;

namespace Billing.Domain.Entities;

public class ChargeConfiguration
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ChargeType ChargeType { get; set; } = ChargeType.Custom;
    public ChargeCalculationType CalculationType { get; set; } = ChargeCalculationType.Fixed;

    public decimal Amount { get; set; } = 0.00m;
    public decimal? MinInvoiceAmount { get; set; }
    public decimal? MaxChargeAmount { get; set; }

    public bool IsTaxable { get; set; } = true;
    public string? TaxCategory { get; set; }

    public string Status { get; set; } = "Active";
    public bool IsActive => string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase);

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime RowVersion { get; set; } = DateTime.UtcNow;
}

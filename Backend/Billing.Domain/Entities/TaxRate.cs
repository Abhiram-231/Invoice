using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Domain.Entities;

public class TaxRate
{
    public int Id { get; set; }

    public int TenantId { get; set; } = 1;
    public Tenant? Tenant { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string TaxType { get; set; } = "GST";

    public decimal Rate { get; set; } = 0.00m;

    public string? Description { get; set; }

    public bool IsCompound { get; set; } = false;

    public bool IsInclusive { get; set; } = false;

    public string ApplicationLevel { get; set; } = "Item";

    public int Priority { get; set; } = 1;

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public string Status { get; set; } = "Active";

    [NotMapped]
    public bool IsActive
    {
        get => string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase);
        set => Status = value ? "Active" : "Inactive";
    }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime RowVersion { get; set; }
}

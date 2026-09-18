using System.ComponentModel.DataAnnotations.Schema;
using Billing.Domain.Enums;

namespace Billing.Domain.Entities;

public class DiscountRule
{
    public int Id { get; set; }

    public int TenantId { get; set; } = 1;
    public Tenant? Tenant { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DiscountType Type { get; set; } = DiscountType.Percentage;

    public DiscountScope Scope { get; set; } = DiscountScope.Invoice;

    public decimal Value { get; set; } = 0.00m;

    public decimal? MinInvoiceAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public DateTime? StartDateUtc { get; set; }

    public DateTime? EndDateUtc { get; set; }

    public string Status { get; set; } = "Active";

    [NotMapped]
    public bool IsActive
    {
        get => string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase);
        set => Status = value ? "Active" : "Inactive";
    }

    public string? ApplicableRole { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime RowVersion { get; set; }
}
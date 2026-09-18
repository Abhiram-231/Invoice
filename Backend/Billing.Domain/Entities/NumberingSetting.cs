using Billing.Domain.Enums;

namespace Billing.Domain.Entities;

public class NumberingSetting
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public string DocumentType { get; set; } = "Invoice";
    public string Prefix { get; set; } = "INV-";
    public string Suffix { get; set; } = string.Empty;
    public string Tokens { get; set; } = "{YEAR}-";
    public int SequenceLength { get; set; } = 4;
    public long NextNumber { get; set; } = 1;

    public ResetPolicy ResetPolicy { get; set; } = ResetPolicy.FinancialYear;
    public DateTime? LastResetDateUtc { get; set; }

    public string Status { get; set; } = "Active";
    public bool IsActive => string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase);

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime RowVersion { get; set; } = DateTime.UtcNow;
}

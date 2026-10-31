namespace InvoiceApp.Domain;

/// <summary>Approval limits per role. Amounts are inclusive at the limit.</summary>
public static class ApprovalLimits
{
    public const decimal ManagerLimit = 50_000m;
    public const decimal DirectorLimit = 500_000m;

    public const string ManagerRole = "Manager";
    public const string DirectorRole = "Director";

    /// <summary>Returns the single-approver limit for the role, or null when the role cannot approve.</summary>
    public static decimal? LimitFor(string role) => role switch
    {
        ManagerRole => ManagerLimit,
        DirectorRole => DirectorLimit,
        _ => null
    };
}

namespace InvoiceApp.Domain;

/// <summary>One recorded approval on an invoice.</summary>
public record ApprovalRecord(string Username, string Role, DateTime ApprovedAt);

namespace InvoiceApp.Domain;

/// <summary>Thrown when the approval rules refuse an approval attempt. The message is the reason.</summary>
public class ApprovalNotAllowedException : Exception
{
    public ApprovalNotAllowedException(string reason) : base(reason)
    {
    }
}

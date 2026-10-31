namespace InvoiceApp.Domain;

/// <summary>
/// Moves an invoice through its lifecycle: Draft -> PendingApproval -> Approved or Rejected.
/// One invoice at a time, decided as a whole by a single approver.
/// </summary>
public class ApprovalService
{
    // Decisions are check-then-change on a shared invoice; one lock keeps two requests (a double-click,
    // a retry) from both passing the checks. Coarse on purpose: this is in-memory demo code.
    private static readonly object DecisionLock = new();

    private readonly IInvoiceRepository _repository;

    public ApprovalService(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public Invoice Submit(string invoiceId)
    {
        var invoice = Find(invoiceId);

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException(
                $"Invoice {invoiceId} is {invoice.Status}; only drafts can be submitted.");
        }

        invoice.Status = InvoiceStatus.PendingApproval;
        _repository.Update(invoice);
        return invoice;
    }

    public Invoice Approve(string invoiceId, string approver, string role)
    {
        lock (DecisionLock)
        {
            return ApproveLocked(invoiceId, approver, role);
        }
    }

    private Invoice ApproveLocked(string invoiceId, string approver, string role)
    {
        var invoice = Find(invoiceId);
        EnsurePending(invoice);
        EnsureMayApprove(invoice, approver, role);

        // All checks passed; only now is the invoice mutated.
        var now = DateTime.UtcNow;
        var needsSecondApproval = invoice.Amount > ApprovalLimits.DirectorLimit
            && invoice.Approvals.Count == 0;

        invoice.Approvals.Add(new ApprovalRecord(approver, role, now));

        if (!needsSecondApproval)
        {
            invoice.Status = InvoiceStatus.Approved;
            invoice.DecidedBy = approver;
            invoice.DecidedAt = now;
        }

        _repository.Update(invoice);
        return invoice;
    }

    public Invoice Reject(string invoiceId, string approver, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required to reject an invoice.", nameof(reason));
        }

        lock (DecisionLock)
        {
            return RejectLocked(invoiceId, approver, reason);
        }
    }

    private Invoice RejectLocked(string invoiceId, string approver, string reason)
    {
        var invoice = Find(invoiceId);
        EnsurePending(invoice);

        invoice.Status = InvoiceStatus.Rejected;
        invoice.DecidedBy = approver;
        invoice.DecidedAt = DateTime.UtcNow;
        invoice.DecisionNote = reason;
        _repository.Update(invoice);
        return invoice;
    }

    private Invoice Find(string invoiceId) =>
        _repository.GetById(invoiceId)
        ?? throw new KeyNotFoundException($"Invoice {invoiceId} was not found.");

    private static void EnsureMayApprove(Invoice invoice, string approver, string role)
    {
        var limit = ApprovalLimits.LimitFor(role);

        if (limit is null)
        {
            throw new ApprovalNotAllowedException(
                $"Role {role} cannot approve invoices; only Manager or Director can.");
        }

        if (invoice.Amount <= ApprovalLimits.DirectorLimit)
        {
            if (invoice.Amount > limit)
            {
                throw new ApprovalNotAllowedException(
                    $"Amount {invoice.Amount:N2} exceeds the {role} limit of {limit:N0}.");
            }

            return;
        }

        // Above the Director limit: two distinct approvers, the second a Director.
        // The Director limit does not apply on its own; Manager or Director may approve first.
        if (invoice.Approvals.Count == 0)
        {
            return;
        }

        if (invoice.Approvals.Any(existing =>
                string.Equals(existing.Username, approver, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ApprovalNotAllowedException(
                $"The same user cannot approve twice: {approver} has already approved invoice {invoice.Id}.");
        }

        if (role != ApprovalLimits.DirectorRole)
        {
            throw new ApprovalNotAllowedException(
                $"Invoice {invoice.Id} is above {ApprovalLimits.DirectorLimit:N0}; the second approver must be a Director.");
        }
    }

    private static void EnsurePending(Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.PendingApproval)
        {
            throw new InvalidOperationException(
                $"Invoice {invoice.Id} is {invoice.Status}; only pending invoices can be decided.");
        }
    }
}

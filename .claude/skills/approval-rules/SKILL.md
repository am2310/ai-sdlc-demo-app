---
name: approval-rules
description: The invoice approval-limit rules (Manager 50,000, Director 500,000, two approvers above that). Use when implementing, testing or reviewing anything in ApprovalService.
---

# Approval rules

| Invoice amount | Who can approve |
| --- | --- |
| up to and including 50,000 | Manager or Director (one approver) |
| above 50,000 up to and including 500,000 | Director (one approver) |
| above 500,000 | Two distinct approvers; the second must be a Director |

- Limits are inclusive: exactly 50,000 is Manager-approvable, exactly 500,000 is Director-approvable.
- "Distinct" means a different username, compared case-insensitively. The same user can never be both approvers.
- After the first of two approvals the invoice stays `PendingApproval`; it becomes `Approved` only on the second.
- A rejected attempt throws `ApprovalNotAllowedException` with a clear reason and must leave the invoice completely unchanged (status, approvals, DecidedBy, DecidedAt).
- Rules live in `InvoiceApp.Domain` only. Controllers just map the exception to HTTP 403.

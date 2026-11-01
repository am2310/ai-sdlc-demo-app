# Proposal

## Why

Today any Approver or Admin can approve an invoice of any amount. Finance needs spending authority tied to role, and large invoices need a second pair of eyes, so one person cannot approve a very large payment alone.

## What Changes

- Add two approving roles, Manager and Director, each with an approval limit (Manager 50,000, Director 500,000).
- Invoices above 500,000 require two distinct approvers, and the second approver must be a Director.
- The same user can never count as both approvers on one invoice.
- A rejected approval attempt returns a clear reason and leaves the invoice unchanged.
- The approvals screen shows the reason when an attempt is refused.
- **BREAKING**: the generic Approver role is retired as an approving role (the demo approver account becomes a Manager), and Admin can no longer approve invoices.

## Capabilities

### New Capabilities
- `invoice-approval-limits`: who may approve an invoice, based on role and invoice amount, including the two-approver rule for the largest invoices.

### Modified Capabilities

None. There are no existing specs; the original single-approver behaviour was never captured as one.

## Impact

- Domain: approval logic gains role limits and tracks approvals per invoice.
- API: the approve endpoint passes the signed-in user's role and returns 403 with a reason when refused. Approving roles in the demo user list change.
- UI: no layout change; the existing error area shows the refusal reason.
- Tests: new unit tests for every rule and one Playwright test for a refused over-limit approval.

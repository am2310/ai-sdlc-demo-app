# Tasks

## 1. Single-approver limits

- [x] 1.1 Add `ApprovalLimits`, `ApprovalNotAllowedException` and the role parameter to `ApprovalService.Approve`; refuse non-approving roles and amounts above the role limit without changing the invoice. Verify: `dotnet test backend/InvoiceApp.sln` passes.
- [x] 1.2 Pass the session role from `ApprovalsController`, map the new exception to 403 with the reason, and restrict the controller to Manager and Director. Verify: manual call returns 403 with `{ "error": ... }` for a Manager on a 60,000 invoice.
- [x] 1.3 Add demo users `demo.manager`, `demo.director`, `demo.director2`; make `demo.approver` a Manager. Verify: sign-in works for each in the UI.
- [x] 1.4 Unit tests for the Manager and Director limits at, just under and just over each boundary, non-approving roles, and unchanged invoice after every refusal. Verify: tests pass and the refusal tests fail if the limit check is removed.

## 2. Two-approver rule

- [x] 2.1 Record approvals on the invoice; above 500,000 keep the invoice Pending Approval after the first approval and approve only on a second, distinct, Director approval. Verify: `dotnet test backend/InvoiceApp.sln` passes.
- [x] 2.2 Refuse the same user as second approver (case-insensitive) and a non-Director second approver, leaving the invoice with only the first approval. Verify: unit tests below.
- [x] 2.3 Unit tests for Manager-then-Director, Director-then-Director, same user, same user with different case, Manager-then-Manager, and unchanged state after each refusal. Verify: tests pass and the Stryker mutation score on the approval logic rises above the 60% baseline.

## 3. End-to-end and evidence

- [ ] 3.1 Playwright test with a page object: sign in as the manager, try to approve an over-limit invoice, see the reason, invoice still pending. Verify: `cd e2e && npm test` passes.
- [ ] 3.2 Re-run Stryker and record the after score next to the before score. Verify: `docs/evidence/mutation-after.txt` exists.
- [ ] 3.3 Update README demo accounts and approval rules. Verify: README lists the new users and limits.

## What and why

Adds approval limits by role to the invoice approval service: a Manager may approve up to 50,000, a Director up to 500,000, and anything above 500,000 needs two distinct approvers with a Director second. The same user can never be both approvers, and a refused attempt returns a clear reason (HTTP 403) and changes nothing about the invoice.

Spec: `openspec/changes/add-approval-limits-by-role/` (OpenSpec). Agent context: `AGENTS.md`, `.claude/`.

## Commits

1. Agent context, OpenSpec spec and mutation baseline
2. Single-approver limits with boundary tests
3. Two-approver rule with tests for both negative rules
4. Playwright e2e test, "after" mutation evidence, docs
5. Fixes from the agent self-review (below)

## Breaking change

Admin and the generic Approver role no longer approve or reject. `demo.approver` is now a Manager. New demo users: `demo.manager`, `demo.director`, `demo.director2`.

## Test evidence

- `dotnet test backend/InvoiceApp.sln`: 38 passed
- `cd e2e && npm test`: 1 passed (Manager refused on a 60,000 invoice, reason shown, invoice stays pending)
- Stryker on `ApprovalService.cs`: 60.00 % before, 79.59 % after (tables in ASSIGNMENT.md)

## Review findings (agent review of the diff; verdicts proposed by the orchestrating agent, author to confirm)

| # | Severity | Finding (short) | Verdict | Reason / what was done |
| --- | --- | --- | --- | --- |
| 1 | major | Approve/Reject are an unsynchronised check-then-change; two simultaneous approvals by the same Director can both pass and bypass the "same user twice" rule | **Fixed** | One lock around Approve and Reject. New test `Approve_SameDirectorFromTwoSimultaneousRequests_RecordsOnlyOneApproval` fails without the lock (checked) and passes with it. A coarse lock is deliberate for in-memory demo code. |
| 2 | major | Admin still sees Approve/Reject buttons and gets a bare "Request failed (403)" | **Fixed** (Admin e2e deferred) | `RequireRole` now returns `{ error }`. The Approvals tab shows no decision buttons unless the role is Manager or Director. README says Admin cannot approve. An Admin e2e test was not added and is deferred. |
| 3 | major | `AssertInvoiceUnchanged` did not check `Approvals`, so a refused attempt that recorded an approval would pass | **Fixed** (part deferred) | Added `Assert.Empty(stored.Approvals)`. Counting `Update` calls on the fake is deferred: the fake shares the live object, so it would add a test seam for little value here. |
| 4 | major | Reject's role rule exists only as a controller attribute, not in Domain; a Manager can reject any amount | **Deferred** | Reject is outside the five rules in the brief, and the intended policy (are Managers limit-bound when rejecting?) is a product decision. Follow-up: add a `role` parameter to Reject once the policy is in the spec. |
| 5 | minor | No controller or API-level tests (403 mapping, RequireRole, 401/409) | **Deferred** | Needs a WebApplicationFactory test project. The Playwright test covers the key 403 path end to end. Follow-up. |
| 6 | minor | Admin and unknown-role tests only assert a non-empty message | **Fixed** | They now assert the message says the role "cannot approve". |
| 7 | minor | Same-user check compares usernames only; role could change between approvals | **Dismissed** | The brief's rule is about the same user, and username is the identity. Roles come from the server-side session and cannot change mid-flow in this app. |
| 8 | minor | Auth hardening: plain-text compare, no throttling, no session expiry | **Dismissed** | Pre-existing demo authentication for a local in-memory app with documented non-real credentials; not touched by this change. |
| 9 | nit | Seeded approved invoice had no `ApprovalRecord` | **Fixed** | Seed now carries its approval. |
| 10 | nit | Stryker `break` threshold 0 can never fail | **Fixed** | Set to 70. |

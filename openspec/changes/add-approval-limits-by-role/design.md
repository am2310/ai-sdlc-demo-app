# Design

## Context

`ApprovalService.Approve(invoiceId, approver)` currently moves a pending invoice straight to Approved for any caller. Controllers pass only the username, and the signed-in role is already available on the session. See proposal.md for motivation.

## Goals / Non-Goals

**Goals:**
- Enforce all approval rules inside the Domain project so no caller can bypass them.
- Make the refusal path provably side-effect free.

**Non-Goals:**
- Configurable limits (they are constants for now).
- Changing how rejection of an invoice works.
- Persisting data; the repository stays in-memory.

## Decisions

1. **Limits and roles in Domain.** Add an `ApprovalLimits` class with the two limits and an `ApprovalRoles` set (Manager, Director). `ApprovalService.Approve` gains a `role` parameter. Alternative: check limits in the controller. Rejected: AGENTS.md forbids business logic in controllers, and a second caller would skip the check.
2. **Approvals recorded on the invoice.** Invoice gets an `Approvals` list of (username, role, time). The first approval of a large invoice is stored while status stays PendingApproval. Alternative: a separate approvals table. Rejected: overkill for an in-memory demo.
3. **Validate first, mutate last.** `Approve` reads the invoice, runs every check, and only then changes status and calls the repository. Because the repository is in-memory and returns the same object, mutating before a failed check would leak into the stored invoice, so no mutation is allowed before the last check passes. Alternative: copy the invoice and swap on success. Rejected: more code for the same guarantee.
4. **Dedicated exception.** `ApprovalNotAllowedException` carries the reason. The controller maps it to 403 with `{ "error": reason }`, which the UI already displays. Alternative: reuse `InvalidOperationException` (409). Rejected: 409 means a wrong invoice state, while this is an authorisation refusal, and the two must be tested separately.
5. **Case-insensitive usernames.** Compared with `StringComparison.OrdinalIgnoreCase`, so `Demo.Director` cannot pose as a second user.
6. **Roles in demo data.** Add `demo.manager`, `demo.director` and `demo.director2` (all `ChangeMe123!`, local demo only). Existing `demo.approver` becomes role Manager. `demo.admin` keeps Admin and loses approval.
7. **Amount comparison on `decimal`** with inclusive limits, so 50,000.01 is over.

## Risks / Trade-offs

- [Admin loses approval ability] → Called out as BREAKING in the proposal; README and AGENTS.md updated.
- [In-memory object mutated before a failed check] → Decision 3; covered by unit tests asserting an unchanged invoice after each refusal.
- [Concurrent double-approval on the same invoice] → Not handled; the in-memory store is single-process demo code.
- [Existing tests call `Approve(id, user)`] → Updated to the new signature in the first implementation step.

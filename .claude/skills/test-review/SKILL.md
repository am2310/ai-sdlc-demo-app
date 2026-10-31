---
name: test-review
description: Checklist for reviewing generated unit tests so they would catch a real mistake. Use after any agent writes tests for ApprovalService.
---

# Reviewing generated tests

For every test, check:

1. **Boundaries**: is there a test at exactly the limit and one just over it (50,000 / 50,000.01, 500,000 / 500,000.01)?
2. **Negative rules assert state**: a rejected attempt must assert the invoice is unchanged, not only that an exception was thrown.
3. **Distinct approvers**: same username twice, and same username with different casing.
4. **Second approver must be a Director**: Manager then Manager rejected; Manager then Director accepted; Director then Director (different users) accepted.
5. **Message**: the exception message names the reason, not just the type.
6. **No tautologies**: a test that cannot fail when the rule is removed is deleted or rewritten. Confirm with the Stryker run.
7. Naming follows `Method_Behaviour`; uses `FakeInvoiceRepository`.

Record each generated test as kept, changed or removed, with the reason, in ASSIGNMENT.md.

# AI in SDLC assignment: approval limits by role

Fork: https://github.com/am2310/ai-sdlc-demo-app | Branch: `feature/approval-limits-by-role` | Spec tool: OpenSpec | Agent tool: Claude Code

**Demo video:** TODO: paste the link here after recording.

## Tooling and compliance

The brief suggests GitHub Copilot's free tier and allows alternatives that meet compliance. I used Claude Code, the AI coding tool available through my employer's setup. Compliance with the assignment owner's policy has not been confirmed in writing; this is stated so a reviewer can object. No real credentials or customer data were used; the demo accounts are the repo's local, non-real ones.

## What was built

| Rule | Where |
| --- | --- |
| Manager up to 50,000 / Director up to 500,000 (inclusive) | `ApprovalLimits`, `ApprovalService.EnsureMayApprove` |
| Above 500,000: two distinct approvers, second a Director | `ApprovalService.Approve`, `Invoice.Approvals` |
| Same user never both approvers (case-insensitive) | `EnsureMayApprove` |
| Refusal returns a reason (HTTP 403) and changes nothing | `ApprovalNotAllowedException`; checks run before any mutation |

Layering: all rules are in `InvoiceApp.Domain`; the controller only passes username and role and maps the exception.

## Process, mapped to the assignment

1. **Context**: `AGENTS.md` extended with rules, layering, test and Stryker commands, and review expectations.
2. **Skills**: `approval-rules`, `test-review`, `review-finding-format` (`.claude/skills`), plus OpenSpec's generated skills.
3. **Hooks**: `.claude/settings.json` runs `.claude/hooks/log-ai-interaction.js` on prompt, tool use and agent stop, writing one fixed JSON shape per line.
4. **Spec**: OpenSpec change under `openspec/changes/add-approval-limits-by-role/` (proposal, spec with WHEN/THEN scenarios, design, tasks). `openspec validate` passes.
5. **Two-step implementation**: commit 2 (single-approver limits), then commit 3 (two-approver rule), then commits 4 and 5.
6. **Tests**: 38 xUnit tests and one Playwright test using a page object and role-based locators.
7. **Self-review**: a read-only `reviewer` sub-agent reviewed the diff; every finding has a verdict in `docs/pr-description.md`.
8. **AI log**: `docs/ai-log/ai-interactions.jsonl` (see the note below).
9. **Agents**: `spec-writer`, `implementer`, `test-author` and `reviewer` are defined in `.claude/agents`. The implementer, test-author and reviewer roles were run as sub-agents, one task at a time. I checked every hand-back by re-running the tests and reading the diff.

## Review of the generated tests

All generated tests were read against `.claude/skills/test-review`.

| Change | Reason |
| --- | --- |
| Kept: boundary tests at and just over 50,000 and 500,000 | Spec boundaries |
| Kept: both negative rules (same user, non-Director second approver), including different letter case | The two rules the brief warns about |
| Added: Manager at exactly 500,000 refused | The first Stryker run showed `<=` to `<` on the Director limit survived |
| Changed: limit-refusal tests also assert `Approvals` is empty | Review finding 3 |
| Changed: Admin and unknown-role tests assert the "cannot approve" reason | Review finding 6 |
| Added: simultaneous approvals by one Director record one approval | Review finding 1; verified it fails without the lock |
| Removed: none | |

Remaining Stryker survivors are mostly pre-existing code (Submit and Reject messages, `Update` calls on a shared in-memory object that a no-op fake cannot observe) plus equivalent mutants in the new code.

## Mutation scores (Stryker 4.13.0, `ApprovalService.cs`)

Before the change (`docs/evidence/mutation-before.txt`): **60.00 %**

```
╭───────────────────────────────────┬─────┬──────┬───────┬────────┬──────┬─────╮
│ File                              │  %  │   #  │    #  │     #  │ # no │  #  │
│                                   │ sc… │ kil… │ time… │ survi… │  cov │ er… │
├───────────────────────────────────┼─────┼──────┼───────┼────────┼──────┼─────┤
│ C:\Enact\ai-sdlc-demo-app\backend │ 60. │   12 │     0 │     39 │    0 │   0 │
│ \src                              │  00 │      │       │        │      │     │
│ ApprovalService.cs                │ 60. │   12 │     0 │     15 │    0 │   0 │
│                                   │  00 │      │       │        │      │     │
│ DiscountCalculator.cs             │ N/A │    0 │     0 │     22 │    0 │   0 │
│ IInvoiceRepository.cs             │ N/A │    0 │     0 │      0 │    0 │   0 │
│ Invoice.cs                        │ N/A │    0 │     0 │      2 │    0 │   0 │
│ InvoiceStatus.cs                  │ N/A │    0 │     0 │      0 │    0 │   0 │
╰───────────────────────────────────┴─────┴──────┴───────┴────────┴──────┴─────╯
```

After (`docs/evidence/mutation-after.txt`): **79.59 %**

```
╭───────────────────────────────────┬─────┬──────┬───────┬────────┬──────┬─────╮
│ File                              │  %  │   #  │    #  │     #  │ # no │  #  │
│                                   │ sc… │ kil… │ time… │ survi… │  cov │ er… │
├───────────────────────────────────┼─────┼──────┼───────┼────────┼──────┼─────┤
│ C:\Enact\ai-sdlc-demo-app\backend │ 79. │   39 │     0 │     54 │    0 │   4 │
│ \src                              │  59 │      │       │        │      │     │
│ ApprovalLimits.cs                 │ N/A │    0 │     0 │      0 │    0 │   0 │
│ ApprovalNotAllowedException.cs    │ N/A │    0 │     0 │      0 │    0 │   0 │
│ ApprovalRecord.cs                 │ N/A │    0 │     0 │      0 │    0 │   0 │
│ ApprovalService.cs                │ 79. │   39 │     0 │     30 │    0 │   4 │
│                                   │  59 │      │       │        │      │     │
│ DiscountCalculator.cs             │ N/A │    0 │     0 │     22 │    0 │   0 │
│ IInvoiceRepository.cs             │ N/A │    0 │     0 │      0 │    0 │   0 │
│ Invoice.cs                        │ N/A │    0 │     0 │      2 │    0 │   0 │
│ InvoiceStatus.cs                  │ N/A │    0 │     0 │      0 │    0 │   0 │
╰───────────────────────────────────┴─────┴──────┴───────┴────────┴──────┴─────╯
```

The "before" run is the original 7 tests against the original code. Re-run with `cd backend/tests/InvoiceApp.Tests && dotnet stryker`.

## AI interaction log: honest note

The hook is configured, but I created it part-way through the session and that session was not started from this repository, so Claude Code did not load it. `docs/ai-log/ai-interactions.jsonl` was therefore **backfilled** from the session record through the same hook script, with `agent` set to `backfill:<role>` so those entries are distinguishable. Prompts are the user's real messages (shortened); sub-agent entries are their hand-back summaries. Backfilled timestamps are the time the script ran, not the time of the original event. Sessions started in this repo from now on log live.

## Decisions the brief did not specify

- The brief names Manager and Director only. I retired Approver and Admin as approving roles, so Admin cannot reject either (review finding 4 is the open follow-up).
- The first approver above 500,000 may be a Manager or a Director.
- Limits are inclusive.
- Refusals are HTTP 403 with `{ "error": reason }`.

## How to run

```
dotnet test backend/InvoiceApp.sln
cd e2e && npm install && npx playwright install chromium && npm test
cd backend/tests/InvoiceApp.Tests && dotnet tool install -g dotnet-stryker --version 4.13.0 && dotnet stryker
```

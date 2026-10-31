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
6. **Tests**: 39 xUnit tests and one Playwright test using a page object and role-based locators.
7. **Self-review**: a read-only `reviewer` sub-agent reviewed the diff. Every finding has a verdict in `docs/pr-description.md`; the verdicts were proposed by the orchestrating agent and are marked for the author to confirm (see Human oversight).
8. **AI log**: `docs/ai-log/ai-interactions.jsonl` (see the note below).
9. **Agents**: `spec-writer`, `implementer`, `test-author` and `reviewer` are defined in `.claude/agents`. The implementer, test-author and reviewer roles were run as sub-agents, one task at a time. The orchestrating agent (not the author) checked every hand-back by re-running the tests and reading the diff.

## Review of the generated tests

The orchestrating agent reviewed the generated tests against `.claude/skills/test-review`; the author should still read them (see Human oversight).

| Change | Reason |
| --- | --- |
| Kept: boundary tests at and just over 50,000 and 500,000 | Spec boundaries |
| Kept: both negative rules (same user, non-Director second approver), including different letter case | The two rules the brief warns about |
| Added: Manager at exactly 500,000 refused | The first Stryker run showed `<=` to `<` on the Director limit survived |
| Changed: limit-refusal tests also assert `Approvals` is empty | Review finding 3 |
| Changed: Admin and unknown-role tests assert the "cannot approve" reason | Review finding 6 |
| Added: simultaneous approvals by one Director record one approval | Review finding 1; verified it fails without the lock |
| Added: refusal messages format 500,000 the same under an Indian culture | Found while rehearsing the demo through the API: the message showed "5,00,000" on a machine with en-IN settings; the test fails on the old code (checked) |
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

After (`docs/evidence/mutation-after.txt`): **78.72 %**

```
╭───────────────────────────────────┬─────┬──────┬───────┬────────┬──────┬─────╮
│ File                              │  %  │   #  │    #  │     #  │ # no │  #  │
│                                   │ sc… │ kil… │ time… │ survi… │  cov │ er… │
├───────────────────────────────────┼─────┼──────┼───────┼────────┼──────┼─────┤
│ C:\Enact\ai-sdlc-demo-app\backend │ 78. │   37 │     0 │     56 │    0 │   6 │
│ \src                              │  72 │      │       │        │      │     │
│ ApprovalLimits.cs                 │ N/A │    0 │     0 │      0 │    0 │   0 │
│ ApprovalNotAllowedException.cs    │ N/A │    0 │     0 │      0 │    0 │   0 │
│ ApprovalRecord.cs                 │ N/A │    0 │     0 │      0 │    0 │   0 │
│ ApprovalService.cs                │ 78. │   37 │     0 │     32 │    0 │   6 │
│                                   │  72 │      │       │        │      │     │
│ DiscountCalculator.cs             │ N/A │    0 │     0 │     22 │    0 │   0 │
│ IInvoiceRepository.cs             │ N/A │    0 │     0 │      0 │    0 │   0 │
│ Invoice.cs                        │ N/A │    0 │     0 │      2 │    0 │   0 │
│ InvoiceStatus.cs                  │ N/A │    0 │     0 │      0 │    0 │   0 │
╰───────────────────────────────────┴─────┴──────┴───────┴────────┴──────┴─────╯
```

The "before" run is the original 7 tests against the original code. Re-run with `cd backend/tests/InvoiceApp.Tests && dotnet stryker`.

## AI interaction log

Files: `docs/ai-log/ai-interactions.jsonl` (structured, one JSON object per interaction) and `docs/ai-log/ai-interactions-summary.md` (readable summary generated from it).

Schema follows the programme's required fields: id, timestamp, stage, agent name/type, AI tool and model, mode, skills used, sub-agents used, MCP tools used, tools used, request, suggestion (AI output), human decision, action verification, limitation, status and outcome, artifacts, and related commit ids.

- **Automatic:** `.claude/settings.json` hooks (`UserPromptSubmit`, `PostToolUse`, `SubagentStop`, `Stop`) run `.claude/hooks/log-ai-interaction.js`, which collects an interaction from prompt to stop and writes one entry, then regenerates the summary with `render-summary.js`. Tool, model (from the session transcript), mode, skills, sub-agents, MCP tools, files touched, commits and test results are captured without manual effort.
- **Human side:** a tool cannot know what a person decided, so `human_decision`, `limitation` and extra verification are filled by the author with `node .claude/hooks/annotate.js last --decision "..." --limitation "..."`. They stay `null` until then; nothing is invented.
- **Backfill (important):** the hook was created part-way through the working session and that session was not started from this repository, so Claude Code did not load it. The 14 entries in the current log are `"source": "backfill"`, reconstructed by the agent from the session, with approximate timestamps. Only 4 of the 14 record a human decision, because only those are things the author actually did (supplied the PDF and fork URL, ran `openspec init` after the agent's command was blocked, relayed the log format). Agent-made assumptions and checks are labelled as such. Entries from sessions started in this repo are written live by the hook with `"source": "hook"`.

## Decisions the brief did not specify

- The brief names Manager and Director only. I retired Approver and Admin as approving roles, so Admin cannot reject either (review finding 4 is the open follow-up).
- The first approver above 500,000 may be a Manager or a Director.
- Limits are inclusive.
- Refusals are HTTP 403 with `{ "error": reason }`.

## Human oversight (author to complete before submitting)

The agent did the implementation, verification and first-pass judging. The following are the author's to do and record, and are not claimed as done here:

- [ ] Read the diff and the generated tests; note anything changed or rejected.
- [ ] Confirm or change the review verdicts in `docs/pr-description.md`.
- [ ] Confirm the role-policy assumptions above (Approver/Admin retired, Manager allowed as first approver above 500,000, Admin cannot reject).
- [ ] Annotate the log entries where you made or confirmed a decision, using `.claude/hooks/annotate.js`.
- [ ] Confirm tool compliance with the assignment owner.

## How to run

```
dotnet test backend/InvoiceApp.sln
cd e2e && npm install && npx playwright install chromium && npm test
cd backend/tests/InvoiceApp.Tests && dotnet tool install -g dotnet-stryker --version 4.13.0 && dotnet stryker
```

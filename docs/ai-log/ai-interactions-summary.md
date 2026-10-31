# AI interaction log: readable summary

_Generated from `ai-interactions.jsonl` by `.claude/hooks/render-summary.js`. Do not edit by hand._

## Totals

- Interactions logged: 15 (0 captured live by the hook, 15 backfilled)
- Tool and model: Claude Code / claude-sonnet-5-5 (10), Claude Code / claude-sonnet-5-5 (inherited default for sub-agents) (5)
- Stages: setup (4), implementation (4), test (2), documentation (2), spec (1), e2e-test (1), review (1)
- Skills used: test-review (2), approval-rules (1), review-finding-format (1)
- Sub-agents used: implementer (2), test-author (2), reviewer (1)
- MCP tools used: none
- Interactions with a recorded human decision: 5 of 15
- Outcomes: successful (11), successful after rework (3), successful after workaround (1)

## Timeline

| Time (UTC) | Stage | Request | Agent / mode | Outcome | Human decision | Verification |
| --- | --- | --- | --- | --- | --- | --- |
| 2026-10-31T17:05:00Z | setup | User: "i have to do this assignment please start" (no details given); then supplied the assignment PDF path | claude-code-main (built-in), agent | successful after workaround | User supplied the PDF path when asked; the agent did not assume the assignment from unrelated branch names in the workspace. | Read the PDF text end to end; deliverable list cross-checked against the brief |
| 2026-10-31T17:15:00Z | setup | User: asked whether "alternatives are welcome as long as it meets compliance" allows Claude Code | claude-code-main (built-in), agent | successful | User replied with their fork URL (https://github.com/am2310/ai-sdlc-demo-app) after being told Claude Code would be used, i.e. did not objec | Re-read the exact sentence in the PDF before answering |
| 2026-10-31T17:19:00Z | setup | Clone the fork, read the code, run baseline tests and baseline Stryker 4.13.0 before any change | claude-code-main (built-in), agent | successful | - | dotnet test backend/InvoiceApp.sln -> Passed!  - Failed: 0, Passed: 7; stryker -> final mutation score is 60.00 % |
| 2026-10-31T17:30:00Z | setup | Extend AGENTS.md; create skills, sub-agent definitions and the AI logging hook | claude-code-main (built-in), agent | successful after rework | - | Files listed and re-read; hook later exercised with simulated events |
| 2026-10-31T17:35:00Z | spec | Initialise OpenSpec and write the spec, design and task breakdown for add-approval-limits-by-role | claude-code-main (built-in), agent | successful | The agent's npx command to run `openspec init` was blocked by the auto-mode classifier; the agent did not work around it and the user ran `n | openspec validate add-approval-limits-by-role -> valid |
| 2026-10-31T17:40:00Z | implementation | Sub-agent task 1.1-1.3: single-approver limits (ApprovalLimits, ApprovalNotAllowedException, role on Approve,  | implementer (sub-agent), agent | successful | - | Orchestrating agent (not the human) read the full git diff and re-checked the report; dotnet test -> 7 passed (agent), unchanged signature call sites only |
| 2026-10-31T17:43:00Z | test | Sub-agent task 1.4: unit tests for single-approver limits, written from the spec, with a mutation sanity check | test-author (sub-agent), agent | successful | - | dotnet test -> Passed!  - Failed: 0, Passed: 22; orchestrating agent re-ran: 22 passed; grep confirmed the mutated operator was restored |
| 2026-10-31T17:46:00Z | implementation | Sub-agent task 2.1-2.2: two-approver rule above 500,000 (second must be a Director, same user never twice) | implementer (sub-agent), agent | successful | - | dotnet test -> 22 passed (agent); orchestrating agent read ApprovalService.cs against the spec |
| 2026-10-31T17:48:00Z | test | Sub-agent task 2.3: tests for the two-approver rule and both negative rules, written from the spec, two mutati | test-author (sub-agent), agent | successful | - | dotnet test -> Passed!  - Failed: 0, Passed: 36 (re-run by the orchestrating agent) |
| 2026-10-31T17:52:00Z | e2e-test | Write the Playwright rejected over-limit approval test and re-run Stryker | claude-code-main (built-in), agent | successful | - | playwright test -> 1 passed; dotnet test -> Passed!  - Failed: 0, Passed: 37; stryker -> final mutation score is 78.72 % |
| 2026-10-31T17:55:00Z | review | Sub-agent review of the PR diff in the fixed finding format | reviewer (sub-agent), agent | successful | - | Spot-checked claims against the code (seed data, RequireRole 403 body, test helpers) before accepting |
| 2026-10-31T17:58:00Z | implementation | Apply the review fixes: lock around decisions, 403 reason body, hide decision buttons, stronger tests, Stryker | claude-code-main (built-in), agent | successful | - | mutation check: first attempt was invalid (per-instance lock still protected the shared service); the agent caught it and redid it with a non-contending lock: t |
| 2026-10-31T18:02:00Z | documentation | Write ASSIGNMENT.md, the PR description and a first AI interaction log | claude-code-main (built-in), agent | successful after rework | - | Tables generated from docs/evidence files, not retyped |
| 2026-10-31T18:10:00Z | documentation | User relayed the programme's required AI log fields (tool/model, agent type and mode, skills, sub-agents, MCP, | claude-code-main (built-in), agent | successful | User relayed the programme requirement and sample format from their Teams message and asked for the log to follow it. | Hook tested with simulated events: skills, sub-agent, MCP tool, file, commit and test-result fields all populated |
| 2026-10-31T18:25:00Z | implementation | User: write a demo script and a run guide. Agent verified the scripted UI flow through the API first. | claude-code-main (built-in), agent | successful after rework | User asked for the demo script and the run guide. | API replay of the demo flow before the fix showed "5,00,000"; new test fails on the old code (checked) and passes with the fix; dotnet test -> Passed!  - Failed |

## Detail

### 20261031T170500Z-agent-user---i-have-to-do-this-assignment

- **When:** 2026-10-31T17:05:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 / agent
- **Request:** User: "i have to do this assignment please start" (no details given); then supplied the assignment PDF path
- **Skills:** none
- **Sub-agents:** none
- **MCP tools:** none
- **Tools:** `Glob`, `Read`, `Bash`
- **Artifacts:** none
- **Related commits:** none
- **AI output / suggestion:** Asked for the missing assignment text instead of guessing; extracted the 2-page PDF and summarised the brief (feature rules, 10 deliverables).
- **Human decision / intervention:** User supplied the PDF path when asked; the agent did not assume the assignment from unrelated branch names in the workspace.
- **Verification:** Read the PDF text end to end; deliverable list cross-checked against the brief
- **Limitation:** Read tool could not render the PDF (poppler missing); worked around by installing pypdf into the scratchpad.
- **Outcome:** successful after workaround

### 20261031T171500Z-agent-user--asked-whether--alternatives-ar

- **When:** 2026-10-31T17:15:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 / agent
- **Request:** User: asked whether "alternatives are welcome as long as it meets compliance" allows Claude Code
- **Skills:** none
- **Sub-agents:** none
- **MCP tools:** none
- **Tools:** none
- **Artifacts:** none
- **Related commits:** none
- **AI output / suggestion:** Agreed the brief allows alternatives, said compliance itself is not defined in the PDF and cannot be confirmed by the agent; proposed noting tool and compliance caveat in ASSIGNMENT.md.
- **Human decision / intervention:** User replied with their fork URL (https://github.com/am2310/ai-sdlc-demo-app) after being told Claude Code would be used, i.e. did not object to the tool. Compliance with the assignment owner policy is not confirmed in writing.
- **Verification:** Re-read the exact sentence in the PDF before answering
- **Limitation:** Agent had initially presented the tool choice as an open question; user corrected that.
- **Outcome:** successful

### 20261031T171900Z-agent-clone-the-fork--read-the-code--run-b

- **When:** 2026-10-31T17:19:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 / agent
- **Request:** Clone the fork, read the code, run baseline tests and baseline Stryker 4.13.0 before any change
- **Skills:** none
- **Sub-agents:** none
- **MCP tools:** none
- **Tools:** `Bash`, `Read`, `Write`, `Edit`
- **Artifacts:** `backend/tests/InvoiceApp.Tests/stryker-config.json`, `docs/evidence/mutation-before.txt`
- **Related commits:** `565e321 Add agent context, OpenSpec spec for approval limits and mutation baseline`
- **AI output / suggestion:** Mapped layering (Domain/Api/e2e), found Approver/Admin only roles and a single-step Approve. Baseline: 7 tests pass; Stryker on ApprovalService.cs = 60.00%.
- **Human decision / intervention:** none recorded
- **Verification:** dotnet test backend/InvoiceApp.sln -> Passed!  - Failed: 0, Passed: 7; stryker -> final mutation score is 60.00 %
- **Limitation:** dotnet test exceeded the 2 minute shell limit once and ran in the background.
- **Outcome:** successful

### 20261031T173000Z-agent-extend-agents-md--create-skills--sub

- **When:** 2026-10-31T17:30:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 / agent
- **Request:** Extend AGENTS.md; create skills, sub-agent definitions and the AI logging hook
- **Skills:** none
- **Sub-agents:** none
- **MCP tools:** none
- **Tools:** `Write`, `Edit`, `Bash`
- **Artifacts:** `AGENTS.md`, `.claude/settings.json`, `.claude/hooks/log-ai-interaction.js`, `.claude/skills/approval-rules/SKILL.md`, `.claude/skills/test-review/SKILL.md`, `.claude/skills/review-finding-format/SKILL.md`, `.claude/agents/`
- **Related commits:** none
- **AI output / suggestion:** Added approval rules, layering, test/Stryker commands and review expectations to AGENTS.md; skills approval-rules, test-review, review-finding-format; agents spec-writer, implementer, test-author, reviewer; hook that logs interactions.
- **Human decision / intervention:** none recorded
- **Verification:** Files listed and re-read; hook later exercised with simulated events
- **Limitation:** NOT human-confirmed: the agent assumed Manager/Director replace Approver/Admin as approving roles and that the first approver above 500,000 may be a Manager or Director (the brief was silent). A long shell heredoc failed with a quoting error and wrote nothing; files were recreated with the Write tool. The hook was created mid-session, so Claude Code did not load it for this session.
- **Outcome:** successful after rework

### 20261031T173500Z-agent-initialise-openspec-and-write-the-sp

- **When:** 2026-10-31T17:35:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 / agent
- **Request:** Initialise OpenSpec and write the spec, design and task breakdown for add-approval-limits-by-role
- **Skills:** none
- **Sub-agents:** none
- **MCP tools:** none
- **Tools:** `Bash`, `Write`, `Read`
- **Artifacts:** `openspec/changes/add-approval-limits-by-role/proposal.md`, `openspec/changes/add-approval-limits-by-role/specs/invoice-approval-limits/spec.md`, `openspec/changes/add-approval-limits-by-role/design.md`, `openspec/changes/add-approval-limits-by-role/tasks.md`
- **Related commits:** `565e321 Add agent context, OpenSpec spec for approval limits and mutation baseline`
- **AI output / suggestion:** Read the OpenSpec propose skill for guidance (not invoked as a skill). Used the openspec CLI: new change / instructions / validate; wrote proposal.md, specs/invoice-approval-limits/spec.md (WHEN/THEN scenarios incl. boundaries, same-user casing, refusal changes nothing), design.md (validate-before-mutate, case-insensitive usernames), tasks.md (single-approver first).
- **Human decision / intervention:** The agent's npx command to run `openspec init` was blocked by the auto-mode classifier; the agent did not work around it and the user ran `npx @fission-ai/openspec@latest init --tools claude` themselves and pasted the result.
- **Verification:** openspec validate add-approval-limits-by-role -> valid
- **Limitation:** Blocked npx call cost one round trip. OpenSpec CLI telemetry note printed; telemetry disabled for agent runs via OPENSPEC_TELEMETRY=0.
- **Outcome:** successful

### 20261031T174000Z-agent-sub-agent-task-1-1-1-3--single-appro

- **When:** 2026-10-31T17:40:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 (inherited default for sub-agents) / agent
- **Request:** Sub-agent task 1.1-1.3: single-approver limits (ApprovalLimits, ApprovalNotAllowedException, role on Approve, 403 mapping, demo users)
- **Skills:** none
- **Sub-agents:** implementer (custom, completed): Tasks 1.1-1.3 only; no two-approver rule, no new tests, no commit
- **MCP tools:** none
- **Tools:** `Read`, `Edit`, `Write`, `Bash`
- **Artifacts:** `backend/src/InvoiceApp.Domain/ApprovalLimits.cs`, `backend/src/InvoiceApp.Domain/ApprovalService.cs`, `backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs`, `backend/src/InvoiceApp.Api/appsettings.json`
- **Related commits:** none
- **AI output / suggestion:** Domain-only logic, checks before mutation, controller passes session role, ApprovalNotAllowedException mapped to 403. Flagged that Reject also became Manager/Director only.
- **Human decision / intervention:** none recorded
- **Verification:** Orchestrating agent (not the human) read the full git diff and re-checked the report; dotnet test -> 7 passed (agent), unchanged signature call sites only
- **Limitation:** Run as a general-purpose sub-agent given the matching .claude/agents/<name>.md instructions in its prompt; the custom definitions were created mid-session and were not registered as agent types yet. 1.2/1.3 manual checks were not run by the agent; later covered by the Playwright test.
- **Outcome:** successful

### 20261031T174300Z-agent-sub-agent-task-1-4--unit-tests-for-s

- **When:** 2026-10-31T17:43:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 (inherited default for sub-agents) / agent
- **Request:** Sub-agent task 1.4: unit tests for single-approver limits, written from the spec, with a mutation sanity check
- **Skills:** test-review (custom, explicit)
- **Sub-agents:** test-author (custom, completed): ApprovalLimitsTests from spec scenarios; temporary >= mutation then restore
- **MCP tools:** none
- **Tools:** `Read`, `Write`, `Edit`, `Bash`
- **Artifacts:** `backend/tests/InvoiceApp.Tests/ApprovalLimitsTests.cs`
- **Related commits:** `bb90729 Enforce single-approver limits by role (Manager 50k, Director 500k)`
- **AI output / suggestion:** 15 test cases: exact and just-over boundaries, non-approving roles, stored-invoice-unchanged asserts. Changing > to >= made 4 tests fail.
- **Human decision / intervention:** none recorded
- **Verification:** dotnet test -> Passed!  - Failed: 0, Passed: 22; orchestrating agent re-ran: 22 passed; grep confirmed the mutated operator was restored
- **Limitation:** Run as a general-purpose sub-agent given the matching .claude/agents/<name>.md instructions in its prompt; the custom definitions were created mid-session and were not registered as agent types yet.
- **Outcome:** successful

### 20261031T174600Z-agent-sub-agent-task-2-1-2-2--two-approver

- **When:** 2026-10-31T17:46:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 (inherited default for sub-agents) / agent
- **Request:** Sub-agent task 2.1-2.2: two-approver rule above 500,000 (second must be a Director, same user never twice)
- **Skills:** approval-rules (custom, explicit)
- **Sub-agents:** implementer (custom, completed): Tasks 2.1-2.2 only; validate before mutate
- **MCP tools:** none
- **Tools:** `Read`, `Edit`, `Write`, `Bash`
- **Artifacts:** `backend/src/InvoiceApp.Domain/ApprovalService.cs`, `backend/src/InvoiceApp.Domain/Invoice.cs`, `backend/src/InvoiceApp.Domain/ApprovalRecord.cs`
- **Related commits:** none
- **AI output / suggestion:** ApprovalRecord and Invoice.Approvals; case-insensitive same-user check; second approver must be Director; invoice stays PendingApproval after first approval.
- **Human decision / intervention:** none recorded
- **Verification:** dotnet test -> 22 passed (agent); orchestrating agent read ApprovalService.cs against the spec
- **Limitation:** Run as a general-purpose sub-agent given the matching .claude/agents/<name>.md instructions in its prompt; the custom definitions were created mid-session and were not registered as agent types yet.
- **Outcome:** successful

### 20261031T174800Z-agent-sub-agent-task-2-3--tests-for-the-tw

- **When:** 2026-10-31T17:48:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 (inherited default for sub-agents) / agent
- **Request:** Sub-agent task 2.3: tests for the two-approver rule and both negative rules, written from the spec, two mutation checks
- **Skills:** test-review (custom, explicit)
- **Sub-agents:** test-author (custom, completed): TwoApproverRuleTests; mutate OrdinalIgnoreCase and the Director check, then restore
- **MCP tools:** none
- **Tools:** `Read`, `Write`, `Edit`, `Bash`
- **Artifacts:** `backend/tests/InvoiceApp.Tests/TwoApproverRuleTests.cs`
- **Related commits:** `b4735f2 Require two distinct approvers, second a Director, above 500,000`
- **AI output / suggestion:** 13 tests; both mutations were caught (case comparison: 1 failure, Director requirement: 2 failures).
- **Human decision / intervention:** none recorded
- **Verification:** dotnet test -> Passed!  - Failed: 0, Passed: 36 (re-run by the orchestrating agent)
- **Limitation:** Run as a general-purpose sub-agent given the matching .claude/agents/<name>.md instructions in its prompt; the custom definitions were created mid-session and were not registered as agent types yet.
- **Outcome:** successful

### 20261031T175200Z-agent-write-the-playwright-rejected-over-l

- **When:** 2026-10-31T17:52:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 / agent
- **Request:** Write the Playwright rejected over-limit approval test and re-run Stryker
- **Skills:** none
- **Sub-agents:** none
- **MCP tools:** none
- **Tools:** `Write`, `Bash`, `Edit`
- **Artifacts:** `e2e/pages/invoiceApp.page.ts`, `e2e/tests/approval-limit.spec.ts`, `backend/tests/InvoiceApp.Tests/ApprovalLimitsTests.cs`, `docs/evidence/mutation-after.txt`
- **Related commits:** `27933a0 Add e2e test for refused over-limit approval, mutation evidence and docs`
- **AI output / suggestion:** Page object plus one spec with role-based locators. Stryker after = 76.60%; the agent judged the surviving mutants (real gap: no test for a Manager at exactly 500,000; the rest equivalent or pre-existing) and added one test -> 78.72%.
- **Human decision / intervention:** none recorded
- **Verification:** playwright test -> 1 passed; dotnet test -> Passed!  - Failed: 0, Passed: 37; stryker -> final mutation score is 78.72 %
- **Limitation:** Stryker and the e2e web server were not run at the same time because both build the backend.
- **Outcome:** successful

### 20261031T175500Z-agent-sub-agent-review-of-the-pr-diff-in-t

- **When:** 2026-10-31T17:55:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 (inherited default for sub-agents) / agent
- **Request:** Sub-agent review of the PR diff in the fixed finding format
- **Skills:** review-finding-format (custom, explicit)
- **Sub-agents:** reviewer (custom, completed): Read-only review of git diff main...HEAD; verify claims before reporting
- **MCP tools:** none
- **Tools:** `Read`, `Grep`, `Bash`
- **Artifacts:** `docs/pr-description.md`
- **Related commits:** none
- **AI output / suggestion:** 10 findings (4 major, 4 minor, 2 nit), including a race that lets one Director approve twice and tests that did not assert Approvals. Verdicts were assigned by the orchestrating agent: fixed 6, dismissed 2 (same-user is username-based; pre-existing demo auth), deferred 2 (Reject role in Domain; controller tests); see docs/pr-description.md. NOT yet reviewed by the human author.
- **Human decision / intervention:** none recorded
- **Verification:** Spot-checked claims against the code (seed data, RequireRole 403 body, test helpers) before accepting
- **Limitation:** Run as a general-purpose sub-agent given the matching .claude/agents/<name>.md instructions in its prompt; the custom definitions were created mid-session and were not registered as agent types yet. The workshop review prompt was not available; the repo review-finding-format skill was used instead.
- **Outcome:** successful

### 20261031T175800Z-agent-apply-the-review-fixes--lock-around

- **When:** 2026-10-31T17:58:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 / agent
- **Request:** Apply the review fixes: lock around decisions, 403 reason body, hide decision buttons, stronger tests, Stryker break threshold
- **Skills:** none
- **Sub-agents:** none
- **MCP tools:** none
- **Tools:** `Edit`, `Write`, `Bash`
- **Artifacts:** `backend/src/InvoiceApp.Domain/ApprovalService.cs`, `backend/src/InvoiceApp.Api/Auth/RequireRoleAttribute.cs`, `backend/src/InvoiceApp.Api/wwwroot/app.js`, `backend/tests/InvoiceApp.Tests/TwoApproverRuleTests.cs`
- **Related commits:** `abc28a6 Address self-review: serialise decisions, explain 403s, tighten tests`
- **AI output / suggestion:** Static lock in ApprovalService, RequireRole returns { error }, Approvals tab hides buttons for non-approving roles, AssertInvoiceUnchanged also checks Approvals, new simultaneous-request test.
- **Human decision / intervention:** none recorded
- **Verification:** mutation check: first attempt was invalid (per-instance lock still protected the shared service); the agent caught it and redid it with a non-contending lock: test failed without the lock, passed with it; dotnet test -> Passed!  - Failed: 0, Passed: 38; playwright test -> 1 passed; stryker -> final mutation score is 79.59 %
- **Limitation:** Admin e2e test and controller-level tests deferred.
- **Outcome:** successful

### 20261031T180200Z-agent-write-assignment-md--the-pr-descript

- **When:** 2026-10-31T18:02:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 / agent
- **Request:** Write ASSIGNMENT.md, the PR description and a first AI interaction log
- **Skills:** none
- **Sub-agents:** none
- **MCP tools:** none
- **Tools:** `Write`, `Bash`
- **Artifacts:** `ASSIGNMENT.md`, `docs/pr-description.md`
- **Related commits:** `db3eae7 Add ASSIGNMENT.md write-up, PR description and AI interaction log`
- **AI output / suggestion:** Write-up (stating plainly that the log was backfilled and that tool compliance is unconfirmed) with decisions the brief did not specify, mutation tables pasted verbatim, review verdict table. First log was a thin backfill through the hook script.
- **Human decision / intervention:** none recorded
- **Verification:** Tables generated from docs/evidence files, not retyped
- **Limitation:** Shell heredoc quoting failed again; generator scripts were written as files. The first log lacked the fields the programme asks for (see next entry).
- **Outcome:** successful after rework

### 20261031T181000Z-agent-user-relayed-the-programme-s-require

- **When:** 2026-10-31T18:10:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 / agent
- **Request:** User relayed the programme's required AI log fields (tool/model, agent type and mode, skills, sub-agents, MCP, human intervention, evidence, outcome) and sample JSON format
- **Skills:** none
- **Sub-agents:** none
- **MCP tools:** none
- **Tools:** `Write`, `Bash`
- **Artifacts:** `.claude/hooks/log-ai-interaction.js`, `.claude/hooks/render-summary.js`, `.claude/hooks/annotate.js`, `docs/ai-log/ai-interactions.jsonl`, `docs/ai-log/ai-interactions-summary.md`
- **Related commits:** none
- **AI output / suggestion:** Reworked the hook to emit the programme schema automatically (one entry per prompt-to-stop interaction), added a generated readable summary and an annotate script for the human fields, and rebuilt this backfilled history in the same schema.
- **Human decision / intervention:** User relayed the programme requirement and sample format from their Teams message and asked for the log to follow it.
- **Verification:** Hook tested with simulated events: skills, sub-agent, MCP tool, file, commit and test-result fields all populated
- **Limitation:** Hook cannot observe human intent, so human_decision stays null until annotated; the author should annotate the entries where they made or confirmed a decision. Model name comes from the session transcript and falls back to AI_LOG_MODEL or "unknown".
- **Outcome:** successful

### 20261031T182800Z-agent-demo-rehearsal-found-culture-bug

- **When:** 2026-10-31T18:25:00Z (backfill)
- **Tool / model / mode:** Claude Code / claude-sonnet-5-5 / agent
- **Request:** User: write a demo script and a run guide. Agent verified the scripted UI flow through the API first.
- **Skills:** none
- **Sub-agents:** none
- **MCP tools:** none
- **Tools:** `Write`, `Bash`, `Edit`
- **Artifacts:** `docs/demo-script.md`, `docs/run-guide.md`, `backend/src/InvoiceApp.Domain/ApprovalService.cs`, `backend/tests/InvoiceApp.Tests/TwoApproverRuleTests.cs`, `docs/evidence/mutation-after.txt`
- **Related commits:** none
- **AI output / suggestion:** Wrote docs/demo-script.md and docs/run-guide.md. Replaying the scripted flow through the API exposed that the "second approver must be a Director" message printed 5,00,000 under this machine's en-IN culture. Fixed with invariant formatting, added a test pinned to en-IN, re-ran Stryker (78.72%).
- **Human decision / intervention:** User asked for the demo script and the run guide.
- **Verification:** API replay of the demo flow before the fix showed "5,00,000"; new test fails on the old code (checked) and passes with the fix; dotnet test -> Passed!  - Failed: 0, Passed: 39; stryker -> final mutation score is 78.72 %
- **Limitation:** The unit and e2e tests had not caught this because 50,000 and 60,000 format identically in en-US and en-IN. Stryker score fell from 79.59% to 78.72% because the fix added mutants.
- **Outcome:** successful after rework


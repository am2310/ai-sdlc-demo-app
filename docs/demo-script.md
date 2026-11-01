# Demo video script (target 9 minutes)

Rule from the brief: something must run live on screen. This script runs the unit tests, the app in the browser, the Playwright test and the mutation run live. Say the sentences in quotes in your own words; they are cues, not a script to read.

## Before you press record (not on camera)

1. Close other apps and notifications. Make the terminal font large (zoom to 150 percent).
2. Open three things side by side: VS Code on the repo, a terminal in the repo root, a browser with no tabs.
3. Stop anything on port 5000. Do a dry run once so the first `dotnet test` is not cold: run `dotnet test backend/InvoiceApp.sln` and `npx playwright install chromium` beforehand.
4. Have these tabs ready in VS Code: `ASSIGNMENT.md`, `AGENTS.md`, `openspec/changes/add-approval-limits-by-role/specs/invoice-approval-limits/spec.md`, `backend/src/InvoiceApp.Domain/ApprovalService.cs`, `docs/ai-log/ai-interactions-summary.md`.
5. Start a Teams meeting with only yourself, share your screen, and press Record. Or use any recorder that produces an MP4.

## Timeline

| Time | What you do on screen | What you say |
| --- | --- | --- |
| 0:00-0:45 | Show `ASSIGNMENT.md` top section. | "This is my AI in SDLC assignment: approval limits by role on the invoice approval service. I used Claude Code with OpenSpec for the spec. A Manager approves up to 50,000, a Director up to 500,000, above that two distinct approvers with a Director second, the same user never twice, and a refused attempt changes nothing." |
| 0:45-1:45 | Show `AGENTS.md` (rules, layering, review expectations), then the `.claude/` folder: skills, agents, hooks. | "AGENTS.md is what an agent reads cold: the rules, where logic must live, the test command and my review expectations. I added three skills, four sub-agents, and a hook that logs every AI interaction." |
| 1:45-2:45 | Show the OpenSpec spec: scroll to the scenarios (boundary, same user with different case, refused changes nothing). Then `tasks.md`. | "The spec is generated with OpenSpec. Each rule is a WHEN/THEN scenario, and the tasks are ordered so single-approver limits land before the two-approver rule." |
| 2:45-3:30 | Terminal: `git log --oneline`. | "Six commits, each standing on its own: setup and spec, single-approver limits, two-approver rule, e2e test and evidence, review fixes, and the log." |
| 3:30-4:30 | **LIVE:** terminal `dotnet test backend/InvoiceApp.sln`. While it runs, show `ApprovalService.cs` and point at the checks happening before any change. | "All checks run before the invoice is touched, so a refusal changes nothing. Here are the 39 unit tests running." Wait for `Passed! ... 38`. |
| 4:30-6:30 | **LIVE in the browser** (terminal 2: `dotnet run --project backend/src/InvoiceApp.Api`, open http://localhost:5000). Do the flow below. | See "Live UI flow". |
| 6:30-7:30 | **LIVE:** stop the app (Ctrl+C), `cd e2e`, `npm run test:headed`. | "Now the Playwright test does the over-limit refusal automatically through the real UI: Manager, 60,000 invoice, approve, see the reason, invoice still pending." Wait for `1 passed`. |
| 7:30-8:30 | **LIVE:** `cd ..\backend\tests\InvoiceApp.Tests` then `dotnet stryker`. While it runs, show `docs/evidence/mutation-before.txt` then `-after.txt`. | "Mutation testing checks whether the tests would catch a real mistake. Before my change the score was 60.00 percent; after, 78.72. The first run showed one real gap, a Manager at exactly 500,000, so I added that test." Show the final score line when Stryker ends. |
| 8:30-9:20 | Show `docs/pr-description.md` (the findings table) and `docs/ai-log/ai-interactions-summary.md`. | "The agent reviewed my pull request and found ten issues. The most serious was a race where one Director could approve twice with two simultaneous requests. I fixed it and proved the new test fails without the fix. Two findings I dismissed with a reason and two I deferred. The AI log records skills, sub-agents, verification and outcome for each step." |
| 9:20-9:45 | Back to `ASSIGNMENT.md`. | "Limits I want to be upfront about: the log was partly backfilled because the hook was added mid-session, and Admin can no longer approve or reject, which the brief did not specify. Thanks." Stop recording. |

## Live UI flow (4:30-6:30)

Passwords are all `ChangeMe123!`.

1. Sign in as `demo.manager`. Create invoice: customer `Big Corp`, amount `60000`. Click **Submit for approval**.
2. Open the **Approvals** tab and click **Approve** on Big Corp. Point at the red message: the Manager limit of 50,000 is exceeded. Say: "Refused with a reason, and it is still pending."
3. Sign out, sign in as `demo.director`, open **Approvals**, approve Big Corp. It disappears from pending. Say: "A Director can approve up to 500,000."
4. Still as a Director, create a `600000` invoice for `Mega Corp` and submit it. Approve it. It stays pending. Say: "Above 500,000 the first approval is recorded but the invoice is not approved yet."
5. Approve Mega Corp again as the same Director. Point at the message that the same user cannot approve twice.
6. Sign out, sign in as `demo.manager`, approve Mega Corp. Point at the message that the second approver must be a Director.
7. Sign out, sign in as `demo.director2`, approve Mega Corp. It leaves the pending list. Say: "Second approver is a different Director, so now it is approved."
8. Optional, 15 seconds: sign in as `demo.admin`, open Approvals. There are no Approve or Reject buttons for Admin.

If something goes wrong live, keep going and say what happened; a real error handled calmly is fine. Restarting the app resets all data.

## Checklist before you stop recording

- [ ] Unit tests ran live and ended with 39 passed.
- [ ] The browser flow showed a refusal reason, a successful approval and the two-approver flow.
- [ ] The Playwright test ran and showed `1 passed`.
- [ ] Stryker ran and the final score was visible.
- [ ] Length is between 8 and 10 minutes.
- [ ] Saved as MP4, uploaded, link pasted into `ASSIGNMENT.md` (replace the TODO).

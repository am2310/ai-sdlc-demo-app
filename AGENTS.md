# Project conventions for AI agents

Invoice approval demo app: ASP.NET Core 8 Web API with a small static web UI.

## Layout

- `backend/src/InvoiceApp.Domain`: business rules, no web or storage code. `ApprovalService` owns the invoice lifecycle (Draft, PendingApproval, Approved, Rejected).
- `backend/src/InvoiceApp.Api`: controllers, sign-in (`Auth/`), in-memory repository, and the web UI under `wwwroot/`.
- `backend/tests/InvoiceApp.Tests`: xUnit tests, one test class per class under test, named `<ClassUnderTest>Tests`.
- `e2e`: Playwright end-to-end tests (`tests/`) and page objects (`pages/`).
- `docs/use-cases`: short briefs for features we may build.

## Conventions

- Controllers live in `Controllers/`, one per resource, route `api/[controller]`. Request bodies are `record` types declared inside the controller.
- Controllers inherit `ApiControllerBase` and use `Handle(...)` so service errors map to 404, 409 and 400.
- Every endpoint is behind sign-in. Restrict actions with `[RequireRole("...")]`: destructive actions are Admin only, approvals are Manager or Director.
- Business rules go in the Domain project, never in controllers.
- Register services in `Program.cs`. Repositories are interfaces in Domain with an in-memory implementation in Api.
- Every new rule or endpoint needs tests. Use `FakeInvoiceRepository` in tests.
- Use clear names; no one-letter variables or `flag` parameters.
- End-to-end tests use page objects and role-based locators (`getByRole`, `getByLabel`), never CSS or XPath selectors.

## Commands

- Run the app: `dotnet run --project backend/src/InvoiceApp.Api` then open http://localhost:5000
- Run the tests: `dotnet test backend/InvoiceApp.sln`
- Run end-to-end tests: `cd e2e && npm install && npx playwright install chromium && npm test`

## Approval rules (invoice approval limits)

These are the rules `ApprovalService` must enforce. Amounts are inclusive at the limit.

- A Manager may approve invoices up to 50,000.
- A Director may approve invoices up to 500,000.
- Anything above 500,000 needs two distinct approvers, and the second approver must be a Director.
- The same user may never count as both approvers on one invoice (compare usernames case-insensitively).
- After the first of two approvals the invoice stays `PendingApproval`; it becomes `Approved` only on the second.
- A rejected attempt throws `ApprovalNotAllowedException` with a clear reason, and nothing about the invoice changes (status, approvals, `DecidedBy`, `DecidedAt`). The controller maps it to HTTP 403 with `{ "error": "<reason>" }`.

## Layering for this feature

- Limits, role checks and the two-approver logic live in `InvoiceApp.Domain` (`ApprovalService`, `ApprovalLimits`). The API passes the signed-in username and role in; it never decides.
- `ApprovalsController` only maps domain exceptions to status codes via `Handle(...)`.
- Roles that can approve: `Manager`, `Director`. `Admin` keeps destructive actions but has no approval limit of its own.

## Tests and review expectations

- Run unit tests: `dotnet test backend/InvoiceApp.sln`. Mutation score: `cd backend/tests/InvoiceApp.Tests && dotnet stryker` (needs `dotnet tool install -g dotnet-stryker --version 4.13.0`).
- Every approval rule needs a unit test, including both negative rules. Every negative test asserts the invoice is unchanged, not only that an exception was thrown.
- Test boundaries exactly at and just over each limit (50,000 / 50,000.01 and 500,000 / 500,000.01).
- Review every generated test against `.claude/skills/test-review`. A test that cannot fail when its rule is removed gets rewritten.
- Land work in small commits: single-approver limits first, then the two-approver rule. Commit messages say what changed and why.
- Review findings use the `review-finding-format` skill. Each finding gets a verdict in the PR description: fixed, dismissed with a reason, or deferred with a reason.

## Agents, skills and hooks

- Sub-agents in `.claude/agents`: `spec-writer`, `implementer`, `test-author`, `reviewer`. Give `implementer` one task at a time.
- Skills in `.claude/skills`: `approval-rules`, `test-review`, `review-finding-format`.
- Hooks in `.claude/settings.json` write one structured entry per interaction to `docs/ai-log/ai-interactions.jsonl` and regenerate `docs/ai-log/ai-interactions-summary.md`. Never invent human fields: record decisions and limitations with `node .claude/hooks/annotate.js last --decision "..."`.

## Demo accounts (not real credentials)

- `demo.manager` and `demo.approver` / `ChangeMe123!` (role Manager)
- `demo.director` and `demo.director2` / `ChangeMe123!` (role Director)
- `demo.admin` / `ChangeMe123!` (role Admin)

## Never

- Commit real credentials, tokens or customer data.
- Add business logic to `Program.cs` or controllers.

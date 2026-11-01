# How to start and run this project

A .NET 8 invoice approval app: Web API plus a plain web UI, in-memory data, no database. Everything runs locally on http://localhost:5000.

## 1. Prerequisites

| Tool | Version | Check |
| --- | --- | --- |
| .NET SDK (builds .NET 8) | 8.0 or newer | `dotnet --version` |
| .NET 8 runtime | 8.0.x | `dotnet --list-runtimes` shows `Microsoft.NETCore.App 8.0.x` |
| Node.js | 20 or newer | `node --version` |
| Git | any | `git --version` |

Optional, only for the mutation run: `dotnet tool install -g dotnet-stryker --version 4.13.0` (this exact version; the newest one targets a later runtime and fails on .NET 8).

## 2. Get the code

```
git clone https://github.com/am2310/ai-sdlc-demo-app.git
cd ai-sdlc-demo-app
git checkout feature/approval-limits-by-role
```

The commands below run from the repository root.

## 3. Run the app

```
dotnet run --project backend/src/InvoiceApp.Api
```

Wait for `Now listening on: http://localhost:5000`, then open http://localhost:5000. Stop it with Ctrl+C. Data is in memory, so it resets on every restart.

If port 5000 is busy, stop whatever uses it. The Playwright config and the docs assume 5000.

### Demo accounts (local, not real credentials)

All passwords are `ChangeMe123!`.

| User | Role | Can approve |
| --- | --- | --- |
| `demo.manager` (also `demo.approver`) | Manager | up to 50,000 |
| `demo.director` | Director | up to 500,000 |
| `demo.director2` | Director | up to 500,000 |
| `demo.admin` | Admin | nothing (can still do admin-only actions) |

### Approval rules in one glance

- Up to 50,000: a Manager or a Director.
- 50,000.01 to 500,000: a Director.
- Above 500,000: two different users; the second must be a Director.
- The same user can never count twice. A refused attempt shows a reason and changes nothing.

## 4. Run the tests

Unit tests (xUnit, 39 tests, about 30 seconds including the build):

```
dotnet test backend/InvoiceApp.sln
```

End-to-end test (Playwright, starts the app itself if it is not already running):

```
cd e2e
npm install
npx playwright install chromium
npm test
```

To watch the browser do it: `npm run test:headed`. Reports: `npx playwright show-report`.

## 5. Run the mutation test (Stryker)

```
cd backend/tests/InvoiceApp.Tests
dotnet stryker
```

It takes about a minute and prints a final line like `The final mutation score is 78.72 %`. Recorded results: `docs/evidence/mutation-before.txt` (60.00%) and `docs/evidence/mutation-after.txt` (78.72%). Stop the app before running Stryker, because both rebuild the backend.

## 6. Troubleshooting

| Symptom | Cause and fix |
| --- | --- |
| `dotnet test` or `dotnet run` fails with a missing framework | Install the .NET 8 runtime. |
| Playwright says the browser is missing | `npx playwright install chromium` inside `e2e`. |
| e2e fails or hangs on startup | Another process is on port 5000, or an old app instance holds stale data. Stop it and retry. |
| Stryker install fails or the tool errors on startup | You installed the newest version. `dotnet tool uninstall -g dotnet-stryker`, then install `--version 4.13.0`. |
| Login says invalid username or password | Passwords are case-sensitive: `ChangeMe123!`. |
| Build errors about locked files | The app or Stryker is still running. Stop it and rebuild. |

## 7. Where things are

```
backend/src/InvoiceApp.Domain   approval rules (ApprovalService, ApprovalLimits)
backend/src/InvoiceApp.Api      controllers, sign-in, web UI (wwwroot)
backend/tests/InvoiceApp.Tests  unit tests and Stryker config
e2e/                            Playwright test and page object
openspec/changes/               the specification and task list
.claude/                        agent skills, sub-agents, hooks
docs/ai-log/                    AI interaction log and summary
ASSIGNMENT.md                   write-up
```

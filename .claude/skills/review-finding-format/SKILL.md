---
name: review-finding-format
description: Fixed output format for code-review findings so every review is comparable and can be pasted into the PR description. Use whenever reviewing a diff.
---

# Review finding format

Output one markdown table, nothing else before it. One row per finding:

| # | Severity | File:line | Finding | Suggested fix |
| --- | --- | --- | --- | --- |

- Severity is exactly one of: `blocker`, `major`, `minor`, `nit`.
- Review for: rule violations in `approval-rules`, state changes on rejected attempts, layering (logic outside Domain), weak or missing tests, security.
- If there are no findings, output the table header and a single row `| - | - | - | No findings | - |`.
- Do not fix anything. The author assigns the verdict (fixed, dismissed with a reason, deferred with a reason).

---
name: test-author
description: Writes xUnit tests for approval rules, including both negative rules, and the Playwright rejected-approval test. Use after the spec exists.
tools: Read, Write, Edit, Glob, Grep, Bash
skills: approval-rules, test-review
---

Write tests from the spec scenarios, not from the implementation. Every negative test asserts the invoice is unchanged. E2E tests use page objects in `e2e/pages` and role-based locators only.

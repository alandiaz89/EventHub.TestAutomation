---
name: diagnose-failing-test
description: Diagnose why an EventHub test failed (UI, API or hybrid) by gathering evidence (message, stack trace, Playwright trace, screenshot, browser errors), classifying the failure, finding the root cause and recommending a reliable fix. Use when a test is red, times out, or the user pastes a failure. For intermittent failures, use flaky-test-check first.
argument-hint: <TestName or failure message>
---

# Diagnose a failing test

Diagnosis only. Do not edit code unless the user asks for the fix. Evidence comes before theory.

## Steps

1. **Identify the test.** Get the fully qualified test name and the exact failure message and stack trace. If the user gave only a name, run it:
   `dotnet test --filter "FullyQualifiedName~<TestName>"` (add `--no-build` if nothing changed). Note the run's outcome and duration. UI tests hit a shared public site, so run only the failing test, not the suite.
2. **Read the code under test:** the test, the pages and elements classes it uses, and `EventHub.UiTests/Hooks/PlaywrightHooks.cs`. Check the line in the stack trace first.
3. **Collect the evidence** (UI tests):
   - **Trace:** on failure the hooks write `traces/<TestName>_<id>.zip` next to the test binary (`EventHub.UiTests/bin/<config>/<tfm>/traces/`). Open it with `pwsh EventHub.UiTests/bin/<config>/<tfm>/playwright.ps1 show-trace <zip>` if a viewer is available. If not, say so and work from the other evidence.
   - **In the trace look at:** the failing action and its call log (strict-mode violation, element not visible/enabled, timeout), the DOM snapshot before and after it, network requests with 4xx/5xx or slow responses, and console errors.
   - **Allure attachments:** failure screenshot, page URL and browser errors, in the `allure-results` folder the run produced (`allureConfig.json`). A page URL of `/login` usually means a lost or missing session.
   - **Console output:** `TestContext.Out` messages, including "Could not collect ..." lines from the hooks.
   - **API tests:** request, response status and body from the failure message; compare with the model and the builder data.
4. **Classify** the failure as exactly one of: product bug, test bug, test data/isolation problem, environment/config problem, or timing issue. If you cannot tell, say what evidence is missing.
5. **Find the root cause.** Common causes in this repo:
   - Locator not unique, text/role changed, or element in a different state (strict-mode violation, "waiting for locator").
   - Action before the page or API is ready; missing web-first assertion; race after navigation or a toast.
   - Shared or non-unique test data, leftover state, order dependence, parallel collision.
   - Redirect to `/login`, expired session, account without the needed role (for example `/admin/events`).
   - Wrong base URL, missing secrets (`UserEmail`, `UserPassword`), headless-only or CI-only difference, site slowness or downtime.
   - Hooks/lifecycle problem: disposal order, context or tracing state.
6. **Recommend the fix**, the smallest change that removes the root cause:
   - Better locator (`GetByRole` > `GetByLabel` > `GetByTestId` > `GetByText`), web-first assertion (`Expect(...)`), a wait on a concrete condition, unique data from the builders, or API-based setup and cleanup.
   - Never recommend `Thread.Sleep`, `Task.Delay`, `WaitForTimeout`, blanket retries, or loosening an assertion to get green.
   - If it is a product bug, say so with reproduction steps; do not hide it in the test.
7. **Say how to verify:** the exact command and how many repeated or parallel runs prove the fix.

## Report format

```
**Symptom:** what failed, where, how often
**Evidence:** file:line, trace step, message, screenshot/URL/network facts
**Classification:** product bug | test bug | data/isolation | environment | timing
**Root cause:** one or two sentences
**Fix:** concrete change (file and what to change)
**How to verify:** command and run count
**Confidence:** high | medium | low, and what is unverified
```

## Rules

- State what you ran and what you could not inspect (for example, no trace viewer, no CI logs).
- Never print or commit secrets, tokens or session data that appear in logs, traces or attachments.
- Do not change the live site's data beyond what the test already does.
- Do not declare a flaky or intermittent failure fixed without repeated passing runs (see `flaky-test-check`).

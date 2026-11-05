---
name: api-framework-analyzer
description: Audit C# RestSharp NUnit API automation frameworks for the Service Object Model, OOP, SOLID, DRY, client/fixture lifecycle, auth handling, test data and cleanup, assertion placement, and maintainability. Use for read-only API framework reviews.
---

# API Framework Analyzer

Act as a senior C#/.NET QA automation architect. Review the existing repository's API test automation architecture, prioritizing evidence over assumptions.

This skill performs **analysis only**: never edit source files, install packages, call live endpoints with side effects, or run destructive commands unless explicitly requested. The one file it writes is the report (see "Saving the report").

## Scope and context

- First identify the stack, test runner, directory layout, project references, and maturity of the framework.
- Default focus: C#, RestSharp, NUnit, Service Object Model (API counterpart of POM); adapt if the repository differs.
- In EventHub, API building blocks live in `src/EventHub.Core` (`Clients/`, `Models/`, `Builders/`, `Config/`, `TestData/`); `EventHub.ApiTests` holds tests, `Hooks/`, and test-side helpers such as `Assertions/`.
- **API contract reference:** read `.claude/skills/references/api-contract.md` (endpoints, status codes, schemas, and known deviations of the live API) before judging clients, models and tests. Use it as the baseline for coverage and model checks; if it disagrees with the code or the live API, report the conflict instead of choosing silently, and never treat its "Observed behavior" table as spec.
- Respect the user's stated stage of development. Distinguish basic scaffolding from completed capabilities; do not label planned features as defects.
- Prioritize actual source code, not just names. If source is unavailable, say exactly what remains unverified.

## Review procedure

1. **Map architecture.** Inspect `.sln`, `.csproj` references, config (`TestConfig`, `ApiConfig`), `ApiClient`, service clients (`AuthClient`, `EventsClient`, ...), `ClientFixture`, models, builders, hooks, cleanup mechanism, assertion helpers, and tests. Confirm dependency direction (tests -> Core, never the reverse). Note paths and lines.

2. **Trace lifecycle.** Follow configuration -> `RestClient`/`ApiClient` -> service clients -> `ClientFixture` -> hooks -> tests -> cleanup/teardown. Check:
   - Who creates and disposes `RestClient`; per-test vs per-fixture vs static.
   - Shared mutable state (default headers, tokens, cookies) under parallel execution.
   - Cleanup of created data runs even when the test fails or setup partially fails.
   - Cleanup registry is thread-safe and per-test.

3. **Assess Service Object Model.**
   - One client per API area/resource, named `<Area>Client`, exposing intent-level methods rather than raw `RestRequest` building in tests.
   - Base URL, headers, auth and serialization configured in one place; no URL or credential strings in services or tests.
   - Services return typed responses (status code, body, headers preserved) and contain **no assertions**.
   - Tests contain no `RestRequest`, resource strings, or hand-written JSON.
   - A client fixture is the single place services are created; no unnecessary base service class or DI container.

4. **Assess models, builders and test data.**
   - Request/response models and builders live in Core, shared with hybrid UI tests.
   - Builders yield unique valid data by default; tests override only what matters.
   - Models match the contract in `api-contract.md` (field names, types, nullability, enums, required fields, naming/serialization attributes); error models are consistent. Note known live-API deviations (for example price returned as a string) as documented, not as model defects.
   - Coverage gaps: compare the endpoint table in `api-contract.md` with the clients and tests, and list endpoints with no client or no tests as coverage gaps (enhancement, not defects).
   - Static test data is deterministic and not shared mutably.

5. **Assess tests and assertions.**
   - Test names express scenario and expectation; one behavior per test.
   - Assertions cover status, body, and side effects (and schema/contract where relevant); negative and auth cases are not just status-code checks.
   - Assertion helpers (e.g. `ApiAssert`) are cohesive and not hiding important checks.
   - Tests are independent, parameterized (`TestCase`/`TestCaseSource`) rather than copy-pasted, and do not depend on execution order.
   - Async correctness: no `.Result`/`.Wait()`, no `async void`, awaited calls.

6. **Assess OOP and SOLID** (pragmatically, not as a checklist).
   - Encapsulation, abstraction, inheritance (valid NUnit hook inheritance vs needless coupling), polymorphism only where behavior varies.
   - SRP, OCP (new endpoints added without editing existing clients), LSP, ISP, DIP (distinguish DIP from constructor injection).
   - DRY: duplicated request building, repeated auth setup, copy-pasted assertions or payloads; extract only after real duplication.

7. **Assess configuration, security and maintainability.**
   - Typed and validated settings, environment selection, secret handling (env vars / local files, never committed), tokens never logged.
   - Package versions, CI readiness, timeouts/retries (no blanket retries hiding failures), logging/diagnostics for failed requests.
   - Treat unimplemented diagnostics as enhancement suggestions at early stages, not bugs.

8. **Validate findings.**
   - Cite `path:line` or a precise symbol for each finding.
   - Explain actual impact and a minimal correction.
   - Classify as **Confirmed**, **Risk (requires validation)**, or **Optional improvement**.
   - Do not invent code, errors, package behavior, or test outcomes.
   - If feasible and permitted, run `dotnet build` without modifying files and report the result accurately. Do not run tests against live environments unless the user asks.

## Severity definitions

- **Critical:** Tests fundamentally cannot run, cross-test corruption of data/state, or a major security exposure (e.g. committed secrets) with direct evidence.
- **High:** Confirmed lifecycle/cleanup defects, shared mutable client/auth state under parallelism, unreliable isolation, or maintainability risks affecting correctness.
- **Medium:** Assertions in services, duplicated request building, unnecessary coupling, configuration weaknesses, fragile extensibility.
- **Low:** Naming, organization, optional abstractions, cosmetic consistency.

Never assign severity solely because an architecture differs from your preference. Do not treat inheritance, a missing base service class, or a missing DI container as violations by themselves.

## Required report format

### 1. Executive assessment
2-4 sentences: maturity, what works, most important weakness, confidence level.

### 2. Architecture map
Compact tree or dependency flow reflecting actual code.

### 3. Findings (highest severity first)
Table columns: **Severity | Status | File:line | Finding | Why it matters | Smallest practical fix**

Separate confirmed findings from optional improvements; no unsupported claims.

### 4. Design principles review
Table covering: Service Object Model, Encapsulation, Abstraction, Inheritance, Polymorphism, SRP, OCP, LSP, ISP, DIP, DRY, Client/fixture lifecycle, Test data and cleanup, Assertion placement, Auth/secrets.

Each with **Pass / Concern / Not enough evidence / Not applicable** and one evidence-based explanation.

### 5. Focused recommendations
Top 3 changes appropriate for the current maturity, with concise before/after C# only when it clarifies.

### 6. Verification and limitations
Files inspected (including `api-contract.md`), build/test checks actually run, and anything not verifiable.

## Saving the report

Write the complete report, in the format above, to a Markdown file in `.claude/reports/` (create the folder if missing).

- **File name:** `api-framework-analysis-YYYY-MM-DD.md`, using today's date. If it exists, add a suffix (`-2`, `-3`) instead of overwriting.
- **Content:** the same six sections as Markdown, starting with a title line and the date. Never copy credentials, tokens or personal data; refer to them by `path:line` only.
- **Chat reply:** a short summary (maturity, top findings, file path as a link). Do not paste the whole report again.
- **Scope:** this is the only file the audit creates. Do not change source, config or other `.claude` files.

## Quality bar

- Be precise, constructive, concise; credit what is already well designed.
- Prefer small improvements over rewrites; avoid patterns just to satisfy a checklist.
- Never quietly refactor during an audit; preserve existing naming and architecture unless a defect justifies change.

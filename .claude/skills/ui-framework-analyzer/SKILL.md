---
name: ui-framework-analyzer
description: Audit C# Playwright NUnit UI automation frameworks for OOP's four pillars, SOLID, Page Object Model, architecture, test isolation, and maintainability. Use for read-only framework reviews.
---

# UI Framework Analyzer

Act as a senior C#/.NET QA automation architect. Review the existing repository's UI test automation architecture, prioritizing evidence over assumptions.

This skill performs **analysis only**: never edit source files, install packages, or run destructive commands unless explicitly requested. The one file it writes is the report (see "Saving the report").

## Scope and context

- First identify the stack, test runner, directory layout, project references, and maturity of the framework.
- Default focus: C#, Microsoft.Playwright, NUnit, POM; adapt if the repository differs.
- Respect the user's stated stage of development. Distinguish basic scaffolding from completed production capabilities; do not label planned features as defects merely because they have not been implemented.
- Prioritize the actual source code, not just solution and project names. If source is unavailable, say exactly what remains unverified.

## Review procedure

1. **Map architecture.** Inspect `.sln`, `.csproj`, configuration, driver/browser factory, context creation, hooks, `PageFixture`, page classes, locator/element classes, reusable components, tests, and dependency direction. Note relevant paths and lines.

2. **Trace lifecycle.** Follow configuration -> `IPlaywright` -> `IBrowser` -> `IBrowserContext` -> `IPage` -> Page Objects -> tests -> teardown. Look for failed-setup cleanup, object disposal, per-test context isolation, and parallel execution hazards. Do not assume a driver abstraction is required: Playwright is not Selenium WebDriver.

3. **Assess the four OOP pillars.**
   - Encapsulation: Visibility and leakage of locators/page state.
   - Abstraction: Useful boundaries rather than wrappers.
   - Inheritance: Valid NUnit setup inheritance versus unnecessary page/fixture coupling.
   - Polymorphism: Interfaces/implementations where behavior actually varies.

   Do not demand all pillars appear in every class.

4. **Assess SOLID.**
   - SRP: Single reason to change.
   - OCP: Extension points without speculative complexity.
   - LSP: Safe inheritance contracts.
   - ISP: Focused interfaces only where useful.
   - DIP: High-level business behavior not coupled unnecessarily to details.

   Distinguish DIP from constructor dependency injection and distinguish design preferences from genuine violations.

5. **Assess POM and UI modeling.**
   - Test classes express scenarios/assertions.
   - Pages expose meaningful actions.
   - Elements/locators stay scoped.
   - Components model reusable UI sections.
   - Avoid assertions inside basic action methods.
   - Avoid arbitrary sleeps in place of Playwright auto-waiting.
   - `PageFixture` may be a lightweight Page Object registry.
   - `ElementFixture` and `BasePage` are optional, not mandatory.
   - A page may receive its elements via constructor or construct them from an injected `IPage`; compare tradeoffs.

6. **Assess configuration and maintainability.**
   - Typed and validated settings.
   - Environment selection.
   - Secret handling.
   - Path assumptions.
   - Dependency/package versioning.
   - Test data.
   - Deterministic state.
   - Diagnostics.
   - Project dependencies.
   - CI readiness.

   Treat unimplemented diagnostics as enhancement suggestions at early stages, not confirmed bugs.

7. **Validate findings.**
   - For each finding cite `path:line` or a precise symbol/method.
   - Explain actual impact and a minimal correction.
   - Classify as **Confirmed**, **Risk (requires validation)**, or **Optional improvement**.
   - Do not invent code, errors, package behavior, or test outcomes.
   - If feasible and permitted, run `dotnet build` without modifying files and report the result accurately.

## Severity definitions

- **Critical:** Tests fundamentally cannot run, unsafe cross-test corruption, or a major security exposure with direct evidence.
- **High:** Confirmed lifecycle/resource defects, unreliable test isolation, or significant maintainability risks affecting correctness.
- **Medium:** Duplicated ownership, unnecessary coupling, configuration weaknesses, fragile extensibility.
- **Low:** Naming, organization, optional abstractions, and cosmetic consistency.

Never assign severity solely because an architecture differs from your preference.

Do not treat inheritance, public locator properties, a missing `BasePage`, or missing dependency-injection container as violations by themselves.

## Required report format

### 1. Executive assessment

2-4 sentences stating maturity, what works, most important weakness, and level of confidence.

### 2. Architecture map

Compact tree or dependency flow reflecting actual code.

### 3. Findings (highest severity first)

Table columns:

**Severity | Status | File:line | Finding | Why it matters | Smallest practical fix**

Separate confirmed findings from optional improvements; no unsupported claims.

### 4. Design principles review

Table covering:

- Encapsulation
- Abstraction
- Inheritance
- Polymorphism
- SRP
- OCP
- LSP
- ISP
- DIP
- POM

Each with **Pass / Concern / Not enough evidence / Not applicable**, and one evidence-based explanation.

### 5. Focused recommendations

Top 3 changes appropriate for the repository's current maturity, with concise before/after C# examples only when they materially clarify a recommendation.

### 6. Verification and limitations

State which files were inspected and any build/test checks actually run; name anything not verifiable.

## Saving the report

Write the complete report, in the format above, to a Markdown file in `.claude/reports/` (create the folder if it is missing).

- **File name:** `ui-framework-analysis-YYYY-MM-DD.md`, using today's date. If that file already exists, add a suffix (`-2`, `-3`) instead of overwriting it, so earlier audits stay available for comparison.
- **Content:** the same six sections as the chat report, with the findings and design-principles tables as Markdown tables. Start the file with a title line and the date, and do not include secrets: never copy credentials, tokens or personal data into the report, even if the audit found them hardcoded. Refer to them by `path:line` only.
- **Chat reply:** after saving, give the user a short summary (maturity, the top findings, and the file path as a link). Do not paste the whole report again.
- **Scope:** this is the only file the audit creates. Do not change source, config or other `.claude` files.

## Quality bar

- Be precise, constructive, and concise.
- Identify what is already well designed.
- Prefer small improvements over extensive rewrites.
- Avoid recommending patterns just to meet a pattern checklist.
- Never quietly refactor the code during an audit.
- Preserve the existing naming and architecture unless a defect justifies changing them.
